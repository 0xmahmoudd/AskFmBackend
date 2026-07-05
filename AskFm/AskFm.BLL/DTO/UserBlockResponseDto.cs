namespace AskFm.BLL.DTO;

public class UserBlockResponseDto
{
    public int Id { get; set; }
    public int BlockedUserId { get; set; }
    public string? BlockedUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}
