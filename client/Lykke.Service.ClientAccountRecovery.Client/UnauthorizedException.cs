using Microsoft.Rest;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    /// <summary>
    /// An exception for 401 code
    /// </summary>
    public class UnauthorizedException : RestException
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }
}