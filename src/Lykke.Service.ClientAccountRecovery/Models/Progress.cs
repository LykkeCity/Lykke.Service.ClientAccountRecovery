namespace Lykke.Service.ClientAccountRecovery.Models
{
    /// <summary>
    /// A current state of the challenge
    /// </summary>
    public enum Progress
    {
        Ongoing,
        WaitingForSupport,
        Frozen,
        Suspended,
        Allowed,
        Undefined
    }
}
