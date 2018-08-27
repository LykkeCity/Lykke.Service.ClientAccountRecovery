using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    /// <summary>
    /// A challenge resolution provided by the support
    /// </summary>
    [PublicAPI]
    public enum CheckResult
    {
        Unknown,
        Approved,
        Rejected
    }
}
