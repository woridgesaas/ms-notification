using System.ComponentModel.DataAnnotations;
using ms_notification.Enum;

public class SendNotificationRequest<T>
{
    [Required(ErrorMessage = "Channel is required")]
    [EnumDataType(typeof(Channel), ErrorMessage = "Invalid channel")]
    public Channel Channel { get; set; }

    [Required(ErrorMessage = "Template code is required")]
    [EnumDataType(typeof(TemplateCode), ErrorMessage = "Invalid template code")]
    public TemplateCode TemplateCode { get; set; }

    [Required(ErrorMessage = "Audience is required")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Audience must be between 5 and 100 characters")]
    public string To { get; set; } = null!;

    [Required(ErrorMessage = "Created by is required")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Created by must be between 5 and 100 characters")]
    public string CreatedBy { get; set; } = null!;

    [Required(ErrorMessage = "Data are required")]
    public T Data { get; set; } = default!;

}

