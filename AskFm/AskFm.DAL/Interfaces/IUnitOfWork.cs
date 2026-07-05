using AskFm.DAL.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Thread = AskFm.DAL.Models.Thread;

namespace AskFm.DAL.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<ApplicationUser> Users { get; }
    IRepository<Thread>  Threads { get; }
    IRepository<SavedThreads> SavedThreads { get; }
    IRepository<ThreadLike>  ThreadLikes { get; }
    IRepository<Comment>  Comments { get; }
    IRepository<CommentLike>  CommentLikes { get; }
    IRepository<Follow>  Follows { get; }
    IRepository<Notification>  Notifications { get; }
    IRepository<UserBlock> UserBlocks { get; }
    IRepository<UserMute> UserMutes { get; }
    
    int Save();
    Task<int> SaveAsync();

    Task<IDbContextTransaction> BeginTransactionAsync();
}