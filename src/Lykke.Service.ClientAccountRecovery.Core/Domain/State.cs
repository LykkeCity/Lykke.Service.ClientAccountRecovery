namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    /// <summary>
    /// A current state of the state machine
    /// </summary>
    public enum State
    {
        RecoveryStarted,
        AwaitSecretPhrases,
        AwaitDeviceVerification,
        AwaitSmsVerification,
        AwaitEmailVerification,
        AwaitSelfieVerification,
        SelfieVerificationInProgress,
        AwaitPinCode,
        PasswordChangeFrozen,
        PasswordChangeSuspended,
        CallSupport,
        Transfer,
        PasswordChangeAllowed,
        PasswordChangeForbidden,
        PasswordUpdated
    }
}
