using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    public interface IChallengeManager
    {
        Task<bool> ExecuteAction(Challenge challenge, Action action, string code, IRecoveryFlowService flow);
    }
}
