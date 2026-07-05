using System.Runtime.InteropServices.JavaScript;

namespace AskFm.DAL.Models;

public class UserMute : ITrackable
{
    public int Id { get; set; }
    
    public int MuterId { get; set; }
    public virtual ApplicationUser? Muter { get; set; }
    
    public int MutedId { get; set; }
    public virtual ApplicationUser? Muted { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime DeletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
