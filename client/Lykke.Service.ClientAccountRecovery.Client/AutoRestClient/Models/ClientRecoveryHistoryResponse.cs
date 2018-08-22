// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.Service.ClientAccountRecovery.Client.AutoRestClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class ClientRecoveryHistoryResponse
    {
        /// <summary>
        /// Initializes a new instance of the ClientRecoveryHistoryResponse
        /// class.
        /// </summary>
        public ClientRecoveryHistoryResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ClientRecoveryHistoryResponse
        /// class.
        /// </summary>
        /// <param name="recoveryId">An id of the recovery</param>
        /// <param name="time">An event date time</param>
        /// <param name="state">An event state. Possible values include:
        /// 'RecoveryStarted', 'AwaitSecretPhrases', 'AwaitDeviceVerification',
        /// 'AwaitSmsVerification', 'AwaitEmailVerification',
        /// 'AwaitSelfieVerification', 'SelfieVerificationInProgress',
        /// 'AwaitPinCode', 'PasswordChangeFrozen', 'PasswordChangeSuspended',
        /// 'CallSupport', 'Transfer', 'PasswordChangeAllowed',
        /// 'PasswordChangeForbidden', 'PasswordUpdated'</param>
        /// <param name="initiator">The initiator of the event</param>
        public ClientRecoveryHistoryResponse(string recoveryId, System.DateTime time, State state, string initiator)
        {
            RecoveryId = recoveryId;
            Time = time;
            State = state;
            Initiator = initiator;
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
        /// Gets an event date time
        /// </summary>
        [JsonProperty(PropertyName = "time")]
        public System.DateTime Time { get; private set; }

        /// <summary>
        /// Gets an event state. Possible values include: 'RecoveryStarted',
        /// 'AwaitSecretPhrases', 'AwaitDeviceVerification',
        /// 'AwaitSmsVerification', 'AwaitEmailVerification',
        /// 'AwaitSelfieVerification', 'SelfieVerificationInProgress',
        /// 'AwaitPinCode', 'PasswordChangeFrozen', 'PasswordChangeSuspended',
        /// 'CallSupport', 'Transfer', 'PasswordChangeAllowed',
        /// 'PasswordChangeForbidden', 'PasswordUpdated'
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public State State { get; private set; }

        /// <summary>
        /// Gets the initiator of the event
        /// </summary>
        [JsonProperty(PropertyName = "initiator")]
        public string Initiator { get; private set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
        }
    }
}
