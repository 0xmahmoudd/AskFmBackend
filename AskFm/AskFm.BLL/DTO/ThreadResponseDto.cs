using AskFm.DAL.Enums;

namespace AskFm.BLL.DTO;

public class ThreadResponseDto
{
    public int Id { get; set; }
    public string QuestionContent { get; set; }
    public string? AnswerContent { get; set; }
    public ThreadStatus Status { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? AskerId { get; set; }
    public string? AskerName { get; set; }
    public string? AskerAvatarPath { get; set; }
    public int AskedId { get; set; }
    public string? AskedName { get; set; }
    public string? AskedAvatarPath { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public DateTime? SavedAt { get; set; }
}