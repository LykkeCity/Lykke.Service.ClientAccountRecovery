namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    public class SelfiePostedEvent
    {
        public string SelfieId { get; set; }
        public string ClientId { get; set; }
        public string RecoveryId { get; set; }
    }
}
