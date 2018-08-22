using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class NewRecoveryResponse
    {
        /// <summary>
        /// An id of the recovery
        /// </summary>
        [DisplayName("clientId")]
        [Required]
        public string RecoveryId { get; internal set; }
    }
}
