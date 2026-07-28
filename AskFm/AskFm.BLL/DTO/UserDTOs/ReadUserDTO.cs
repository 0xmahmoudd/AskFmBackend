namespace AskFm.BLL.DTO.UserDTOs;

public class ReadUserDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime LastSeen { get; set; }
    public string Bio { get; set; }
    public string AvatarPath { get; set; }
    public int followerCount { get; set; }
}