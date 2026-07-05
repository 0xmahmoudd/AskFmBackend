using System.ComponentModel.DataAnnotations;

namespace AskFm.BLL.DTO.UserDTOs;

public class UpdateEmailDto
{
    [Required]
    [EmailAddress]
    public string NewEmail { get; set; }
}
