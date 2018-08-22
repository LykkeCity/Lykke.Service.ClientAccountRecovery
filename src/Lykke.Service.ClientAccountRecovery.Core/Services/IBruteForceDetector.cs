using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    public interface IBruteForceDetector
    {
        Task<bool> IsNewRecoveryAllowedAsync(string clientId);
        Task<IReadOnlyCollection<RecoveryUnit>> GetRecoveriesToSeal(string clientId);
    }
}
