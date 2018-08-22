using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ClientAccountRecovery.Client 
{
    public class ClientAccountRecoveryServiceClientSettings 
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUrl {get; set;}

        [Optional]
        public string ApiKey { get; set; }
    }
}
