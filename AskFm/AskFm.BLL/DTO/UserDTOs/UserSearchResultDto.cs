namespace AskFm.BLL.DTO.UserDTOs;

public class UserSearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public string AvatarPath { get; set; }
    public string Bio { get; set; }
    public int FollowersCount { get; set; }
}
