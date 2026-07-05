using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Identity;

namespace AskFm.DAL.Models;

public class ApplicationUser : IdentityUser<int>, ITrackable
{
    public string Name { get; set; }
    public string Bio { get; set; }
    public string AvatarPath { get; set; }
    
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    
    public DateTime LastSeen { get; set; }

    public virtual ICollection<Thread>? AskedThreads { get; set; }
    public virtual ICollection<Thread>? ReceivedThreads { get; set; }
    public virtual ICollection<Comment>? Comments { get; set; }
    public virtual ICollection<Follow>? Followers { get; set; }
    public virtual ICollection<Follow>? Following { get; set; }
    public virtual ICollection<ThreadLike>? ThreadLikes { get; set; }
    public virtual ICollection<CommentLike>? CommentLikes { get; set; }
    public virtual ICollection<Notification>? Notifications { get; set; }
    public virtual ICollection<SavedThreads>? SavedThreads { get; set; }
    
    public virtual ICollection<UserBlock>? BlockedUsers { get; set; }
    public virtual ICollection<UserBlock>? BlockedByUsers { get; set; }
    public virtual ICollection<UserMute>? MutedUsers { get; set; }
    public virtual ICollection<UserMute>? MutedByUsers { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime DeletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
}