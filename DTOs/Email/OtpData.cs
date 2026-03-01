using System.ComponentModel.DataAnnotations;

namespace ms_notification.DTOs.Email;

public class OtpData
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Full name must be between 5 and 50 characters")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "OTP code is required")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must be exactly 6 digits")]
    public string OtpCode { get; set; } = null!;
}
