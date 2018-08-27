using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ClientAccountRecovery.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        [AzureTableCheck]
        public string RecoveryActivitiesConnString { get; set; }

        [AzureTableCheck]
        public string ClientPersonalInfoConnString { get; set; }
    }
}
