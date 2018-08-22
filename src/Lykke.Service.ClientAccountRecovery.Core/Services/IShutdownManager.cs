using System.Threading.Tasks;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
