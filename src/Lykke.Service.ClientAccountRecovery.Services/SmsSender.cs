using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.ConfirmationCodes.Client.Models.Request;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class SmsSender : ISmsSender
    {
        private readonly IConfirmationCodesClient _confirmationCodesClient;
        private readonly IClientAccountClient _accountClient;

        public SmsSender(IClientAccountClient accountClient, IConfirmationCodesClient confirmationCodesClient)
        {
            _accountClient = accountClient;
            _confirmationCodesClient = confirmationCodesClient;
        }


        public async Task SendCodeAsync(string clientId)
        {
            var clientModel = await _accountClient.GetByIdAsync(clientId);
            if (clientModel == null)
            {
                throw new InvalidOperationException($"The inconsistent state. Unable to find a client with id {clientId}");
            }
            await _confirmationCodesClient.SendSmsConfirmationAsync(new SendSmsConfirmationRequest
            {
                Phone = clientModel.Phone,
                PartnerId = clientModel.PartnerId,
                IsPriority = false
            });
        }
    }
}
