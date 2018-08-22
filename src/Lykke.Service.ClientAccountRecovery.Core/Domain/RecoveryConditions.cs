using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    public class RecoveryConditions
    {
        public double FrozenPeriodInDays { get; set; }

        [Optional]
        public int SmsCodeMaxAttempts { get; set; } = 3;

        [Optional]
        public int PinCodeMaxAttempts { get; set; } = 3;

        [Optional]
        public int EmailCodeMaxAttempts { get; set; } = 3;

        [Optional]
        public int SecretPhrasesMaxAttempts { get; set; } = 3;

        [Optional]
        public int MaxUnsuccessfulRecoveryAttempts { get; set; } = 3;
    }
}
