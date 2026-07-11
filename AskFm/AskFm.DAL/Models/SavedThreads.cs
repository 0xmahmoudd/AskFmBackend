using System.Runtime.InteropServices.JavaScript;

namespace AskFm.DAL.Models;

public class SavedThreads : ITrackable
{
    public int SavedThreadId { get; set; }
    public int UserId { get; set; }
    
    public virtual Thread? Thread { get; set; }
    public virtual ApplicationUser? User { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}