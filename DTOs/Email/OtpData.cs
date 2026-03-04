using System.ComponentModel.DataAnnotations;

namespace ms_notification.DTOs.Email;

public class OtpData
{
    

    [Required(ErrorMessage = "OTP code is required")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must be exactly 6 digits")]
    public string OtpCode { get; set; } = null!;
}
