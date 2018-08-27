using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.ClientAccountRecovery.AzureRepositories
{
    public interface IRecoveryStateRepository
    {
        Task<IEnumerable<StateTableEntity>> GetAsync(string clientId);
        Task InsertOrReplaceAsync(string clientId, string recoveryId);
        Task DeleteAsync(string clientId, string recoveryId);
    }
}