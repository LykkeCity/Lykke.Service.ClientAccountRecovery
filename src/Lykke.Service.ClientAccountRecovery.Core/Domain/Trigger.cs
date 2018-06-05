﻿namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    public enum Trigger
    {
        RecoveryRequest,
        SecretPhrasesComplete,
        SecretPhrasesSkip,
        DeviceVerificationComplete,
        DeviceVerificationSkip,
        SmsVerificationComplete,
        SmsVerificationRestart,
        SmsVerificationFail,
        SmsVerificationSkip,
        EmailVerificationComplete,
        EmailVerificationRestart,
        EmailVerificationFail,
        EmailVerificationSkip,
        PinComplete,
        PinSkip,
        SelfieVerificationRequest,
        SelfieVerificationComplete,
        SelfieVerificationSkip,
        SelfieVerificationFail,
        JumpToSuspended,
        JumpToCallSupport,
        JumpToFrozen,
        JumpToAllowed

    }
}
