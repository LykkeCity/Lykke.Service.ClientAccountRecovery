using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class ResolutionRequest
    {
        /// <summary>
        /// An id of the recovery
        /// </summary>
        [Required]
        [MinLength(Consts.MinRecoveryIdLength)]
        public string RecoveryId { get;  set; }

        /// <summary>
        /// A resolution from support
        /// </summary>
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public Resolution Resolution { get;  set; }

        /// <summary>
        /// Client's ip
        /// </summary>
        [Required]
        public string AgentId { get;  set; }

        /// <summary>
        /// Client's user agent
        /// </summary>
        [MaxLength(256)]
        public string Comment { get;  set; }

    }
}
