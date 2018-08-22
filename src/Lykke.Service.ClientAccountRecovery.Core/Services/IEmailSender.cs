using System.Threading.Tasks;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    public interface IEmailSender
    {
        Task SendCodeAsync(string clientId);
    }
}
