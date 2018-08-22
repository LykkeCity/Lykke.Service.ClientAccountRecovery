using System.Threading.Tasks;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    public interface ISmsSender
    {
        Task SendCodeAsync(string clientId);
    }
}
