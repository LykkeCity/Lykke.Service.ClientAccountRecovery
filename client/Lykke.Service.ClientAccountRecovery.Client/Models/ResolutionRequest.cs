// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.Service.ClientAccountRecovery.Client.Models
{
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class ResolutionRequest
    {
        /// <summary>
        /// Initializes a new instance of the ResolutionRequest class.
        /// </summary>
        public ResolutionRequest()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ResolutionRequest class.
        /// </summary>
        /// <param name="resolution">Possible values include: 'Unknown',
        /// 'Suspend', 'Interview', 'Freeze', 'Allow'</param>
        public ResolutionRequest(Resolution resolution, string recoveryId = default(string), string agentId = default(string), string comment = default(string))
        {
            RecoveryId = recoveryId;
            Resolution = resolution;
            AgentId = agentId;
            Comment = comment;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "recoveryId")]
        public string RecoveryId { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Unknown', 'Suspend',
        /// 'Interview', 'Freeze', 'Allow'
        /// </summary>
        [JsonProperty(PropertyName = "resolution")]
        public Resolution Resolution { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "agentId")]
        public string AgentId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "comment")]
        public string Comment { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Comment != null)
            {
                if (Comment.Length > 256)
                {
                    throw new ValidationException(ValidationRules.MaxLength, "Comment", 256);
                }
            }
        }
    }
}
