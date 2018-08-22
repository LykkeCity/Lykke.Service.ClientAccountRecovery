using JetBrains.Annotations;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.Settings.ServiceSettings;
using Lykke.Service.ClientAccountRecovery.Settings.SlackNotifications;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.Kyc.Client;
using Lykke.Service.PersonalData.Settings;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ClientAccountRecovery.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings
    {
        public ClientAccountRecoverySettings ClientAccountRecoveryService { get; set; }

        public SlackNotificationsSettings SlackNotifications { get; set; }

        [Optional]
        public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }

        public ClientAccountServiceClientSettings ClientAccountClient { get; set; }

        public ConfirmationCodesServiceClientSettings ConfirmationCodesClient { get; set; }

        public KycServiceClientSettings KycServiceClient { get; set; }

        public PersonalDataServiceClientSettings PersonalDataServiceClient { get ; set; }
    }
}
