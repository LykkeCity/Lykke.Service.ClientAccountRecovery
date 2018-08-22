// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.Service.ClientAccountRecovery.Client.AutoRestClient.Models
{
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class ChallengeRequest
    {
        /// <summary>
        /// Initializes a new instance of the ChallengeRequest class.
        /// </summary>
        public ChallengeRequest()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ChallengeRequest class.
        /// </summary>
        /// <param name="recoveryId">An id of the recovery</param>
        /// <param name="challenge">A challenge to contest. Possible values
        /// include: 'Unknown', 'Sms', 'Email', 'Selfie', 'Words', 'Device',
        /// 'Pin', 'Undefined'</param>
        /// <param name="action">Resolution. Possible values include:
        /// 'Undefined', 'Complete', 'Restart', 'Skip'</param>
        /// <param name="value">A challenge value</param>
        /// <param name="ip">Client's ip</param>
        /// <param name="userAgent">Client's user agent</param>
        public ChallengeRequest(string recoveryId, Challenge challenge, Action action, string value, string ip, string userAgent)
        {
            RecoveryId = recoveryId;
            Challenge = challenge;
            Action = action;
            Value = value;
            Ip = ip;
            UserAgent = userAgent;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets an id of the recovery
        /// </summary>
        [JsonProperty(PropertyName = "recoveryId")]
        public string RecoveryId { get; set; }

        /// <summary>
        /// Gets or sets a challenge to contest. Possible values include:
        /// 'Unknown', 'Sms', 'Email', 'Selfie', 'Words', 'Device', 'Pin',
        /// 'Undefined'
        /// </summary>
        [JsonProperty(PropertyName = "challenge")]
        public Challenge Challenge { get; set; }

        /// <summary>
        /// Gets or sets resolution. Possible values include: 'Undefined',
        /// 'Complete', 'Restart', 'Skip'
        /// </summary>
        [JsonProperty(PropertyName = "action")]
        public Action Action { get; set; }

        /// <summary>
        /// Gets or sets a challenge value
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets client's ip
        /// </summary>
        [JsonProperty(PropertyName = "ip")]
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets client's user agent
        /// </summary>
        [JsonProperty(PropertyName = "userAgent")]
        public string UserAgent { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (RecoveryId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "RecoveryId");
            }
            if (Value == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Value");
            }
            if (Ip == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Ip");
            }
            if (UserAgent == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "UserAgent");
            }
            if (RecoveryId != null)
            {
                if (RecoveryId.Length < 8)
                {
                    throw new ValidationException(ValidationRules.MinLength, "RecoveryId", 8);
                }
            }
            if (Value != null)
            {
                if (Value.Length > 128)
                {
                    throw new ValidationException(ValidationRules.MaxLength, "Value", 128);
                }
            }
            if (UserAgent != null)
            {
                if (UserAgent.Length > 128)
                {
                    throw new ValidationException(ValidationRules.MaxLength, "UserAgent", 128);
                }
            }
        }
    }
}
