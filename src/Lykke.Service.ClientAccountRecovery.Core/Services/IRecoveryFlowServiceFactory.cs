using System.Threading.Tasks;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    public interface IRecoveryFlowServiceFactory
    {
        Task<IRecoveryFlowService> InitiateNew(string clientId);
        Task<IRecoveryFlowService> FindExisted(string recoveryId);
    }
}
