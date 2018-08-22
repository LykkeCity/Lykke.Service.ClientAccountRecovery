using Microsoft.Rest;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    /// <summary>
    /// An exception for 403 code
    /// </summary>
    public class ForbiddenException : RestException
    {
        public ForbiddenException(string message) : base(message)
        {
        }
    }
}