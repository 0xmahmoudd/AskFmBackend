using System.Runtime.InteropServices.JavaScript;
using AskFm.DAL.Enums;
namespace AskFm.DAL.Models;

public class Thread : ITrackable
{
    public int Id { get; set; }
    public string QuestionContent { get; set; }
    public string AnswerContent { get; set; }

    public ThreadStatus Status { get; set; }
    public bool isAnonymous { get; set; }
    public int? AskerId { get; set; }
    public virtual ApplicationUser? Asker { get; set; }

    public int AskedId { get; set; }
    public virtual ApplicationUser? Asked { get; set; }

    public virtual ICollection<Comment>? Comments { get; set; }
    public virtual ICollection<ThreadLike>? ThreadLikes { get; set; }
    public virtual ICollection<SavedThreads>? SavedThreads { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}