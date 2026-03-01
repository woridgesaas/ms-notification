using System.ComponentModel.DataAnnotations;

namespace ms_notification.DTOs.Email;

public class CreateUserData
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Full name must be between 5 and 50 characters")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Email must be between 5 and 100 characters")]
    [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters")]
    public string Password { get; set; } = null!;
}
