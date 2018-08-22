namespace Lykke.Service.ClientAccountRecovery.Core
{
    public static class Consts
    {
        public const int MinClientIdLength = 8;
        public const int MaxUserAgentLength = 1024;
        public const int MinRecoveryIdLength = 8;
        public const string InitiatorUser = "User";
        public const string InitiatorService = "RecoveryService";
        public const string BoundedContext = "client-account-recovery";
    }
}
