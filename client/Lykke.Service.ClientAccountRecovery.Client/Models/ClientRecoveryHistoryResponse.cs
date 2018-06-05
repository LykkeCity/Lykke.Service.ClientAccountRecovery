// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.Service.ClientAccountRecovery.Client.Models
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
        /// <param name="state">Possible values include: 'RecoveryStarted',
        /// 'AwaitSecretPhrases', 'AwaitDeviceVerification',
        /// 'AwaitSmsVerification', 'AwaitEmailVerification',
        /// 'AwaitKycVerification', 'KycInProgress', 'AwaitPinCode',
        /// 'PasswordChangeFrozen', 'PasswordChangeSuspended', 'CallSupport',
        /// 'Transfer', 'PasswordChangeAllowed',
        /// 'PasswordChangeForbidden'</param>
        public ClientRecoveryHistoryResponse(System.DateTime time, State state, string recoveryId = default(string), string initiator = default(string))
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
        /// </summary>
        [JsonProperty(PropertyName = "recoveryId")]
        public string RecoveryId { get; private set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "time")]
        public System.DateTime Time { get; private set; }

        /// <summary>
        /// Gets possible values include: 'RecoveryStarted',
        /// 'AwaitSecretPhrases', 'AwaitDeviceVerification',
        /// 'AwaitSmsVerification', 'AwaitEmailVerification',
        /// 'AwaitKycVerification', 'KycInProgress', 'AwaitPinCode',
        /// 'PasswordChangeFrozen', 'PasswordChangeSuspended', 'CallSupport',
        /// 'Transfer', 'PasswordChangeAllowed', 'PasswordChangeForbidden'
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public State State { get; private set; }

        /// <summary>
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
