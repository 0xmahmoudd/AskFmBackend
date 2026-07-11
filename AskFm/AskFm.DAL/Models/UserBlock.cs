using System.Runtime.InteropServices.JavaScript;

namespace AskFm.DAL.Models;

public class UserBlock : ITrackable
{
    public int Id { get; set; }
    
    public int BlockerId { get; set; }
    public virtual ApplicationUser? Blocker { get; set; }
    
    public int BlockedId { get; set; }
    public virtual ApplicationUser? Blocked { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
