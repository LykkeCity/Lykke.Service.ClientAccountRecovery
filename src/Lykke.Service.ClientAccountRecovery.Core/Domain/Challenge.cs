using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    /// <summary>
    /// Available client challenges
    /// </summary>
    [PublicAPI]
    public enum Challenge
    {
        Unknown = 0,
        Sms = 1,
        Email = 2,
        Selfie = 3,
        Words = 4,
        Device = 5,
        Pin = 6,
        Undefined = 7
    }
}
