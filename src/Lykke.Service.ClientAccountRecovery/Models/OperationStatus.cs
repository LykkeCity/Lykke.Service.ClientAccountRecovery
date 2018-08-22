namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class OperationStatus
    {
        /// <summary>
        /// True if there was an error
        /// </summary>
        public bool Error { get; internal set; }

        /// <summary>
        /// A description of the error or null
        /// </summary>
        public string Message { get; internal set; }
    }
}
