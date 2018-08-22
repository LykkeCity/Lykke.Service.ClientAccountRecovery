using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ClientAccountRecovery.Settings
{
    [UsedImplicitly]
    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string MonitoringServiceUrl { get; set; }
    }
}
