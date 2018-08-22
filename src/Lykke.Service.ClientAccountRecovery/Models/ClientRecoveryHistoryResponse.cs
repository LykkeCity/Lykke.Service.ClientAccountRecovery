using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class ClientRecoveryHistoryResponse
    {
        /// <summary>
        /// An id of the recovery
        /// </summary>
        [Required]
        public string RecoveryId { get; internal set; }

        /// <summary>
        /// An event date time
        /// </summary>
        [Required]
        public DateTime Time { get; internal set; }

        /// <summary>
        /// An event state
        /// </summary>
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; internal set; }  
        
        /// <summary>
        /// The initiator of the event
        /// </summary>
        [Required]
        public string Initiator { get; internal set; }
    }
}
