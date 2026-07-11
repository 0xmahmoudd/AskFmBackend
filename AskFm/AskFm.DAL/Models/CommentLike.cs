using System.Runtime.InteropServices.JavaScript;

namespace AskFm.DAL.Models;

public class CommentLike : ITrackable
{
    public int CommentId { get; set; }
    public virtual Comment? Comment { get; set; }
    public int UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}