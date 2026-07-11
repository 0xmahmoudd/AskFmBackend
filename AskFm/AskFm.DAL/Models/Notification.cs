using System.Runtime.InteropServices.JavaScript;
using AskFm.DAL.Enums;

namespace AskFm.DAL.Models;

public class Notification : ITrackable
{
    public NotificationStatus Type { get; set; }
    public int Id { get; set; }
    public int UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }
    public bool IsRead { get; set; }
    
    public int ResourceId { get; set; }
    public string Message { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}