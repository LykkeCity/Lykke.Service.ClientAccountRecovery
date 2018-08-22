using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Core
{
    public interface IChallengesValidator
    {
        Task<bool> Confirm(IRecoveryFlowService flowService, string code);
    }
}
