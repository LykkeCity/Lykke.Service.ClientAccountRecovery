using Microsoft.Rest;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    /// <summary>
    /// An exception for 404 code
    /// </summary>
    public class NotFoundException : RestException
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}