using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class RecoveryTraceResponse
    {
        /// <summary>
        /// A date time of the event
        /// </summary>
        [Required]
        public DateTime Time { get; internal set; }

        /// <summary>
        /// A previous state
        /// </summary>
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public State PreviousState { get; internal set; }

        /// <summary>
        /// An action that leaded state changing
        /// </summary>
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public Trigger Action { get; internal set; }

        /// <summary>
        /// A current state
        /// </summary>
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public State NewState { get; internal set; }

        /// <summary>
        /// An initiator of the event
        /// </summary>
        [Required]
        public string Initiator { get; internal set; }

        /// <summary>
        /// Comment from the support
        /// </summary>
        public string Comment { get; internal set; }

        /// <summary>
        /// Client's ip
        /// </summary>
        [Required]
        public string Ip { get;internal set; }

        /// <summary>
        /// Client's user agent
        /// </summary>
        [MaxLength(Consts.MaxUserAgentLength)]
        public string UserAgent { get;internal set; }



        internal static IEnumerable<RecoveryTraceResponse> Convert(RecoveryUnit unit)
        {
            var result = new List<RecoveryTraceResponse>();
            var first = new RecoveryTraceResponse { PreviousState = State.RecoveryStarted };
            result.Add(first);
            var counter = 0;

            // Log is always sorted by SeqNo
            foreach (var context in unit.Log)
            {
                var prev = result[counter];
                prev.Time = context.Time;
                prev.Action = context.Action;
                prev.Comment = context.Comment;
                prev.Initiator = context.Initiator;
                prev.NewState = context.State;
                prev.Ip = context.Ip;
                prev.UserAgent = context.UserAgent;
                var next = new RecoveryTraceResponse { PreviousState = context.State };
                result.Add(next);
                counter++;
            }

            return result.Take(result.Count - 1);
        }
    }


}
