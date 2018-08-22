using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ClientAccountRecovery.Settings.SlackNotifications
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AzureQueuePublicationSettings
    {
        [AzureQueueCheck]
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }
}
