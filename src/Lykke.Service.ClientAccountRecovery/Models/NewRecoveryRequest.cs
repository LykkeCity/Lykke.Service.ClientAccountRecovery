using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class NewRecoveryRequest
    {

        /// <summary>
        /// An id of Client
        /// </summary>
        [Required]
        [MinLength(Consts.MinClientIdLength)]
        public string ClientId { get; set; }

        /// <summary>
        /// Client's ip
        /// </summary>
        [Required]
        public string Ip { get; set; }


        /// <summary>
        /// Client's user agent
        /// </summary>
        [MaxLength(Consts.MaxUserAgentLength)]
        public string UserAgent { get; set; }
    }
}
