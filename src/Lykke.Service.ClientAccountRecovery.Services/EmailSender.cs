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
    public class EmailSender : IEmailSender
    {
        private readonly IConfirmationCodesClient _confirmationClient;
        private readonly IClientAccountClient _accountClient;

        public EmailSender(IClientAccountClient accountClient, IConfirmationCodesClient confirmationClient)
        {
            _accountClient = accountClient;
            _confirmationClient = confirmationClient;
        }

        public async Task SendCodeAsync(string clientId)
        {
            var clientModel = await _accountClient.GetByIdAsync(clientId);
            if (clientModel == null)
            {
                throw new InvalidOperationException($"The inconsistent state. Unable to find a client with id {clientId}");
            }
            await _confirmationClient.SendEmailConfirmationAsync(new SendEmailConfirmationRequest
            {
                Email = clientModel.Email,
                PartnerId = clientModel.PartnerId,
                IsPriority = false
            });
        }
    }
}
