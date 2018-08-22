using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ClientAccountRecoverySettings
    {
        public DbSettings Db { get; set; }

        public string ApiKey { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }

        public RecoveryConditions RecoveryConditions { get; set; }

    }
}
