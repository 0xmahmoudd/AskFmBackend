using System.ComponentModel.DataAnnotations;

namespace AskFm.BLL.DTO.UserDTOs;

public class ConfirmEmailChangeDto
{
    [Required]
    [EmailAddress]
    public string NewEmail { get; set; }

    [Required]
    public string Token { get; set; }
}
