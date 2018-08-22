using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class ApproveChallengeRequest
    {
        /// <summary>
        /// An id of the recovery
        /// </summary>
        [Required]
        [MinLength(Consts.MinRecoveryIdLength)]
        public string RecoveryId { get;  set; }


        /// <summary>
        /// A challenge to approve
        /// </summary>
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public Challenge Challenge { get;  set; }

        /// <summary>
        /// An agent id
        /// </summary>
        [Required]
        public string AgentId { get;  set; }

        /// <summary>
        /// Resolution
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [Required]
        public CheckResult CheckResult { get;  set; }

    }
}
