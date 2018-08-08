﻿using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class SecretPhrasesValidator : PrivateKeValidatorBase, IChallengesValidator
    {
        public SecretPhrasesValidator(IWalletCredentialsRepository credentialsRepository) : base(credentialsRepository)
        {

        }

        public async Task<bool> Confirm(IRecoveryFlowService flowService, string code)
        {
            var clientId = flowService.Context.ClientId;
            var publicKeyAddress = await PublicKeyAddress(clientId);
            if (publicKeyAddress == null)
            {
                throw new InvalidOperationException($"Unable to validate signature because the client with Id {clientId} has no address in the credentials");
            }

            if (VerifyMessage(publicKeyAddress, flowService.Context.SignChallengeMessage, code))
            {
                await flowService.SecretPhrasesCompleteAsync();
                return true;
            }

            await flowService.SecretPhrasesVerificationFailAsync();
            return false;
        }
    }
}
