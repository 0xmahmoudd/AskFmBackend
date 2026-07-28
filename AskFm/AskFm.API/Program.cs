using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AskFm.DAL;
using AskFm.DAL.Models;
using AskFm.DAL.Interfaces;
using AskFm.DAL.Repositories;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Proxies;
using AskFm.BLL.Hub;
using AskFm.BLL.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AskFm.BLL.Services.UserIdentityService;
using Swashbuckle.AspNetCore.SwaggerGen;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.AspNetCore.Identity.UI.Services;
using Shared;
using IEmailSender = AskFm.BLL.Services.IEmailSender;

namespace AskFm.API;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        Env.Load();
        string ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
            ?? builder.Configuration.GetConnectionString("DefaultConnection");

        if (ConnectionString is null)
        {
            throw new Exception("Connection string is null");
        }

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpContextAccessor();
        // DbContext
        builder.Services.AddDbContext<AppDbContext>(options =>
            options
                .UseLazyLoadingProxies()
                .UseSqlServer(ConnectionString));
        // -------------------------------------------------
        // Register the repositories and services
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        builder.Services.AddScoped<INotificationService, NotificationService>();

        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IThreadService, ThreadService>();
        builder.Services.AddScoped<IThreadLikeService, ThreadLikeService>();
        builder.Services.AddScoped<ICommentLikeService, CommentLikeService>();
        builder.Services.AddScoped<ICommentService, CommentService>();
        builder.Services.AddScoped<IEmailSender,EmailSender>();
        builder.Services.AddScoped<IUserModerationService, UserModerationService>();
        builder.Services.AddControllers();

        // Configure Swagger with JWT Authentication
        builder.Services.AddSwaggerGen(setup =>
        {
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

            setup.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtSecurityScheme, Array.Empty<string>() }
            });

        });


        // Authentication & Authorization

        JwtOptions jwtOptions = new JwtOptions
        {
            Issuer = Environment.GetEnvironmentVariable("ISSUER"),
            Audience = Environment.GetEnvironmentVariable("AUDIENCE"),
            SigningKey = Environment.GetEnvironmentVariable("SIGNINGKEY"),
            AccessExpiration = builder.Configuration.GetValue<int>("ExpireTimes:Jwt_Token_Exp"),
            AccessRefreshTokenExpiration =builder.Configuration.GetValue<int>("ExpireTimes:Refresh_Token_Exp")
        };
        if (jwtOptions == null)
        {
            throw new Exception("jwtOptions is null");
        }

        // Enhanced SignalR Configuration
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        });

        // CORS Configuration for SignalR
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("SignalRPolicy", policy =>
            {
                // Option 1: Allow any origin (for development only)
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();

                // Option 2: Specific origins (uncomment and modify when you know frontend URLs)
                // policy.WithOrigins(
                //     "http://localhost:3000",    // React default
                //     "http://localhost:4200",    // Angular default
                //     "http://localhost:8080",    // Vue default
                //     "http://localhost:5173",    // Vite default
                //     "https://yourdomain.com"    // Production domain
                // )
                // .AllowAnyMethod()
                // .AllowAnyHeader()
                // .AllowCredentials();
            });
        });

        builder.Services.Configure<JwtOptions>(Options =>
        {
            Options.Issuer = Environment.GetEnvironmentVariable("ISSUER");
            Options.Audience = Environment.GetEnvironmentVariable("AUDIENCE");
            Options.SigningKey = Environment.GetEnvironmentVariable("SIGNINGKEY");
            Options.AccessExpiration = builder.Configuration.GetValue<int>("ExpireTimes:Jwt_Token_Exp");
            Options.AccessRefreshTokenExpiration = builder.Configuration.GetValue<int>("ExpireTimes:Refresh_Token_Exp");
        });
        builder.Services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(2));

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;


            })
            .AddJwtBearer( Options =>
            {
                Options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ClockSkew = TimeSpan.FromMinutes(0)
                };
                Options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var jti = context.Principal?.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
                        if (jti != null)
                        {
                            var redis = context.HttpContext.RequestServices.GetRequiredService<RedisCacheService>();
                            var cachedToken = await redis.GetCacheAsync<int>(AppConstants.JwtCacheKey(jti));
                            if (cachedToken <= 0)
                            {
                                context.Fail("Token revoked or expired");
                            }
                        }
                    },
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        builder.Services.AddAuthorization();


       // Identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
            {
                //password configuration
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                //Email
                options.User.RequireUniqueEmail = true;

                // Lockout
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.AllowedForNewUsers = true;

                // sign in options
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                /*
                 * close confirmed email imediatly in register,
                 * but in other scenario we will block some action untill the user verify his email
                 */
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Redis Cache
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
            options.InstanceName = "AskFmCache";
        });

        builder.Services.AddSingleton<RedisCacheService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "api");
            });
        }

        app.UseResponseCompression();
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        // Apply CORS before authentication
        app.UseCors("SignalRPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Map SignalR Hub
        app.MapHub<NotificationHub>("/notificationHub");

        app.Run();
    }
}
