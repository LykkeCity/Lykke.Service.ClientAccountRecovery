using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class DeviceValidator : PrivateKeValidatorBase, IChallengesValidator
    {

        public DeviceValidator(IWalletCredentialsRepository credentialsRepository, ILogFactory logFactory) : base(credentialsRepository, logFactory)
        {
        }

        public async Task<bool> Confirm(IRecoveryFlowService flowService, string code)
        {
            var clientId = flowService.Context.ClientId;
            var publicKeyAddress = await PublicKeyAddress(clientId);
            if (string.IsNullOrWhiteSpace(publicKeyAddress))
            {
                throw new InvalidOperationException($"Unable to validate signature because the client with Id {clientId} has no address in the credentials");
            }

            if (VerifyMessage(publicKeyAddress, flowService.Context.SignChallengeMessage, clientId, code))
            {
                await flowService.DeviceVerifiedCompleteAsync();
                return true;
            }

            await flowService.DeviceVerificationFailAsync();
            return false;
        }
    }
}
