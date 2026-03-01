using ms_notification.Enum;
using ms_notification.Exceptions;

namespace ms_notification.Extensions;

public class TemplateCodeExtension
{
      public static string GetSubject(TemplateCode templateCode)
    {
        return templateCode switch
        {
            TemplateCode.OTP => "Your Verification Code",
            TemplateCode.USER_CREATED => "Welcome to Woridge",
            _ => throw new TemplateCodeNotFoundException("The following template code : " + templateCode + " was not found")
        };
    }
}
