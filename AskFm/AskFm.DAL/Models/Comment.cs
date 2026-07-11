using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Identity;

namespace AskFm.DAL.Models;

public class Comment : ITrackable
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int LikeCount { get; set; }

    public int? UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }

    public int ThreadId { get; set; }
    public virtual Thread? Thread { get; set; }

    public int? ParentCommentId { get; }
    public virtual Comment? ParentComment { get; set; }

    public virtual ICollection<Comment>? Replies { get; set; }
    public virtual ICollection<CommentLike>? CommentLikes { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}