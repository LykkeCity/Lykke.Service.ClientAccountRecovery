using System;
using Lykke.AzureStorage.Tables;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.AzureRepositories
{

    public class LogTableEntity : AzureTableEntity
    {

        private const string KeyFormat = "00000000000"; // Never change! The sorting of key depends on this
        public string RecoveryId => PartitionKey;
        public int SeqNo => int.Parse(RowKey);
        public string ClientId { get; set; }

        public DateTime Time { get; set; }
        public State State { get; set; }
        public Trigger Action { get; set; }
        public bool HasSecretPhrases { get; set; }
        public bool DeviceVerified { get; set; }
        public bool DeviceVerificationRequested { get; set; }
        public bool SmsVerified { get; set; }
        public bool EmailVerified { get; set; }
        public bool SelfieApproved { get; set; }
        public bool HasPin { get; set; }
        public int SmsRecoveryAttempts { get; set; }
        public int EmailRecoveryAttempts { get; set; }
        public string Initiator { get; set; }
        public string Comment { get; set; }
        public string Ip { get; set; }
        public string UserAgent { get; set; }
        public DateTime? FrozenDate { get; set; }
        public string SignChallengeMessage { get; set; }
        public int SecretPhrasesRecoveryAttempts { get; set; }
        public int PinRecoveryAttempts { get; set; }

        public static string GetPartitionKey(string recoveryId)
        {
            return recoveryId;
        }

        public static string GetRowKey(int seqNo)
        {
            return seqNo.ToString(KeyFormat);
        }
    }
}
