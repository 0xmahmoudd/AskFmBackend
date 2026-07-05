using AskFm.DAL.Interfaces;
using AskFm.DAL.Models;
using AskFm.DAL.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Thread = AskFm.DAL.Models.Thread;

namespace AskFm.DAL;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IRepository<ApplicationUser> _users;
    private IRepository<Thread> _threads;
    private IRepository<SavedThreads> _savedThreads;
    private IRepository<ThreadLike> _threadLikes;
    private IRepository<Comment> _comments;
    private IRepository<CommentLike> _commentLikes;
    private IRepository<Follow> _follows;
    private IRepository<Notification> _notifications;
    private IRepository<UserBlock> _userBlocks;
    private IRepository<UserMute> _userMutes;


    
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }
    public IRepository<ApplicationUser> Users
    {
        get
        {
            if (_users == null)
            {
                _users = new Repository<ApplicationUser>(_context);
            }
            return _users;
        }
    }

    public IRepository<Thread> Threads {
        get
        {
            if (_threads == null)
            {
                _threads = new Repository<Thread>(_context);
            }
            return _threads;
        }
    }

    public IRepository<SavedThreads> SavedThreads
    {
        get
        {
            if (_savedThreads == null)
            {
                _savedThreads = new Repository<SavedThreads>(_context);
            }
            return _savedThreads;
        }
    }

    public IRepository<ThreadLike> ThreadLikes
    {
        get
        {
            if (_threadLikes == null)
            {
                _threadLikes = new Repository<ThreadLike>(_context);
            }
            return _threadLikes;
        }
    }

    public IRepository<Comment> Comments
    {
        get
        {
            if (_comments == null)
            {
                _comments = new Repository<Comment>(_context);
            }
            return _comments;
        }
    }

    public IRepository<CommentLike> CommentLikes
    {
        get
        {
            if (_commentLikes == null)
            {
                _commentLikes = new Repository<CommentLike>(_context);
            }
            return _commentLikes;
        }
    }

    public IRepository<Follow> Follows
    {
        get
        {
            if (_follows == null)
            {
                _follows = new Repository<Follow>(_context);
            }
            return _follows;
        }
    }

    public IRepository<Notification> Notifications
    {
        get
        {
            if (_notifications == null)
            {
                _notifications = new Repository<Notification>(_context);
            }
            return _notifications;
        }
    }

    public IRepository<UserBlock> UserBlocks
    {
        get
        {
            if (_userBlocks == null)
            {
                _userBlocks = new Repository<UserBlock>(_context);
            }
            return _userBlocks;
        }
    }

    public IRepository<UserMute> UserMutes
    {
        get
        {
            if (_userMutes == null)
            {
                _userMutes = new Repository<UserMute>(_context);
            }
            return _userMutes;
        }
    }


    public void Dispose()
    {
        _context.Dispose();
    }
    
    public int Save()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }
    
}