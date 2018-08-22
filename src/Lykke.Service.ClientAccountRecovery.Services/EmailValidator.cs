using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.ConfirmationCodes.Client.Models.Request;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class EmailValidator : IChallengesValidator
    {
        private readonly IConfirmationCodesClient _confirmationClient;
        private readonly IClientAccountClient _accountClient;

        public EmailValidator(IConfirmationCodesClient confirmationClient, IClientAccountClient accountClient)
        {
            _confirmationClient = confirmationClient;
            _accountClient = accountClient;
        }

        public async Task<bool> Confirm(IRecoveryFlowService flowService, string code)
        {
            var clientModel = await _accountClient.GetByIdAsync(flowService.Context.ClientId);
            if (clientModel == null)
            {
                throw new InvalidOperationException($"The inconsistent state. Unable to find a client with id {flowService.Context.ClientId}");
            }
            var result = await _confirmationClient.VerifyEmailCodeAsync(new VerifyEmailConfirmationRequest
            {
                Email = clientModel.Email,
                PartnerId = clientModel.PartnerId,
                Code = code
            });

            if (result.IsValid)
            {
                await flowService.EmailVerificationCompleteAsync();
                return true;
            }

            await flowService.EmailVerificationFailedAsync();
            return false;
        }
    }
}
