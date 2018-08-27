using Microsoft.Rest;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    /// <summary>
    /// An exception for 500 code
    /// </summary>
    public class ServerErrorException : RestException
    {
        public ServerErrorException(string message) : base(message)
        {
        }
    }
}