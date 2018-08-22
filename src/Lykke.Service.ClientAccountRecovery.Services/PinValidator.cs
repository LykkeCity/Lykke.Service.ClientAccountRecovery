using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class PinValidator : IChallengesValidator
    {
        private readonly IClientAccountClient _accountClient;

        public PinValidator(IClientAccountClient accountClient)
        {
            _accountClient = accountClient;
        }

        public async Task<bool> Confirm(IRecoveryFlowService flowService, string code)
        {
            var isValid = await _accountClient.IsPinValidAsync(flowService.Context.ClientId, code);

            if (isValid)
            {
                await flowService.PinCodeVerificationCompleteAsync();
                return true;
            }

            await flowService.PinCodeVerificationFailAsync();
            return false;
        }
    }
}
