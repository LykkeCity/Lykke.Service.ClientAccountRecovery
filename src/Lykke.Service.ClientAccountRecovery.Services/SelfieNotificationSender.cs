using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class SelfieNotificationSender : ISelfieNotificationSender
    {
        private readonly ICqrsEngine _cqrsEngine;

        public SelfieNotificationSender(ICqrsEngine cqrsEngine)
        {
            _cqrsEngine = cqrsEngine;
        }

        public Task Send(IRecoveryFlowService flow, string code)
        {
            var msg = new SelfiePostedEvent
            {
                ClientId = flow.Context.ClientId,
                RecoveryId = flow.Context.RecoveryId,
                SelfieId = code
            };
            _cqrsEngine.PublishEvent(msg, Consts.BoundedContext);
            
            return flow.SelfieVerificationRequestAsync();
        }
    }
}
