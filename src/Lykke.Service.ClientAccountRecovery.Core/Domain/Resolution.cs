using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    /// <summary>
    /// A resolution from the support
    /// </summary>
    [PublicAPI]
    public enum Resolution
    {
        Unknown,
        Suspend,
        Interview,
        Freeze,
        Allow
    }
}
