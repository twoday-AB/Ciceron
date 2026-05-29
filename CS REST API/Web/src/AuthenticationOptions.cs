using System;
using System.Collections.Generic;

class AuthenticationOptions
{
    public enum MinRegistrationLevelEnum
    {
        EXTENDED,
        PLUS
    }

    public enum CallInitiatorEnum
    {
        USER,
        RP
    }

    public string System { get; set; }
    public string Provider { get; set; }
    public string PersonalNumber { get; set; }
    public string Country { get; set; }
    public string CertificateIssuer { get; set; }
    public Nullable<MinRegistrationLevelEnum> MinRegistrationLevel { get; set; } = null;
    public Nullable<CallInitiatorEnum> CallInitiator { get; set; } = null;
}
