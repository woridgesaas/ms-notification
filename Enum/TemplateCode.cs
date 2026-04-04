namespace ms_notification.Enum;

public enum TemplateCode
{
    OTP = 1,
    USER_CREATED = 2,
    CUSTOM = 3,
    RESIGNATION_CONFIRMATION = 4,
    ABSENCE_ALERT_USER = 5,  // email vers l'employé absent
    ABSENCE_ALERT_RH     = 6,  // notification interne RH + Manager
    ABSENCE_ALERT_CEO   = 7,   // notification interne CEO

    OVERTIME_ALERT_EMPLOYEE = 8

}
