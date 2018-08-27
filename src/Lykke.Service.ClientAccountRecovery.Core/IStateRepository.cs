using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Core
{
    public interface IStateRepository
    {
        Task InsertAsync(RecoveryContext context);
        Task<RecoveriesSummaryForClient> FindRecoverySummary(string clientId);
    }
}
