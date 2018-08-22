using System.Threading.Tasks;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    public interface ISelfieNotificationSender
    {
        Task Send(IRecoveryFlowService flow, string code);
    }
}
