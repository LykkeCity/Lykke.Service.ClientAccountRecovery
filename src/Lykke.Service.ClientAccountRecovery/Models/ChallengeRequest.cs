using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class ChallengeRequest
    {
        /// <summary>
        /// An id of the recovery
        /// </summary>
        [Required]
        [MinLength(Consts.MinRecoveryIdLength)]
        public string RecoveryId { get; set; }

        /// <summary>
        /// A challenge to contest
        /// </summary>
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public Challenge Challenge { get; set; }

        /// <summary>
        /// Resolution
        /// </summary>
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public Action Action { get; set; }

        /// <summary>
        /// A challenge value
        /// </summary>
        [MaxLength(128)]
        public string Value { get; set; } = null;

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
