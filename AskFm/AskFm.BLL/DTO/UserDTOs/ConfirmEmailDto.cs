using System.ComponentModel.DataAnnotations;

namespace AskFm.BLL.DTO.UserDTOs;

public class ConfirmEmailDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Token { get; set; }
}
