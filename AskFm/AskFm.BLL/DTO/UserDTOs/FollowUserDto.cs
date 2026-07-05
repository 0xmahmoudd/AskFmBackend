namespace AskFm.BLL.DTO.UserDTOs;

public class FollowUserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public string AvatarPath { get; set; }
    public string Bio { get; set; }
    public DateTime FollowedSince { get; set; }
}
