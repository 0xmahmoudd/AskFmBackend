namespace AskFm.BLL.DTO;

public class CommentResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int? UserId { get; set; }
    public string UserName { get; set; }
    public string? UserAvatarPath { get; set; }
    public int ThreadId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikesCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
}