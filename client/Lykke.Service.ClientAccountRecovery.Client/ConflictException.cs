using Microsoft.Rest;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    /// <summary>
    /// An exception for 409 code
    /// </summary>
    public class ConflictException : RestException
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}