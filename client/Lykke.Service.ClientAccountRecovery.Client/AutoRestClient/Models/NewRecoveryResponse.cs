// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.Service.ClientAccountRecovery.Client.AutoRestClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class NewRecoveryResponse
    {
        /// <summary>
        /// Initializes a new instance of the NewRecoveryResponse class.
        /// </summary>
        public NewRecoveryResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the NewRecoveryResponse class.
        /// </summary>
        /// <param name="recoveryId">An id of the recovery</param>
        public NewRecoveryResponse(string recoveryId)
        {
            RecoveryId = recoveryId;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets an id of the recovery
        /// </summary>
        [JsonProperty(PropertyName = "recoveryId")]
        public string RecoveryId { get; private set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            //Nothing to validate
        }
    }
}
