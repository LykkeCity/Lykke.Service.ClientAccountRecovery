using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.ClientAccountRecovery.Core;
using NBitcoin;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    public abstract class PrivateKeValidatorBase
    {
        private readonly IWalletCredentialsRepository _credentialsRepository;
        private readonly ILog _log;

        protected PrivateKeValidatorBase(IWalletCredentialsRepository credentialsRepository, ILogFactory logFactory)
        {
            _credentialsRepository = credentialsRepository;
            _log = logFactory.CreateLog(this);
        }

        protected async Task<string> PublicKeyAddress(string clientId)
        {
            var credentials = await _credentialsRepository.GetAsync(clientId);

            // We should never be here, because the state machine must bypass this step
            if (credentials == null)
            {
                throw new InvalidOperationException($"Unable to find a credentials for client {clientId}");
            }
            var publicKeyAddress = credentials.Address;
            return publicKeyAddress;
        }

        protected bool VerifyMessage(string pubKeyAddress, string message, string clientId, string signedMessage)
        {
            var address = new BitcoinPubKeyAddress(pubKeyAddress);
            try
            {
                return address.VerifyMessage(message, signedMessage);
            }
            catch (Exception ex)
            {
                _log.Warning($"Unable to verify the signed message. Client id {clientId}", ex);
                return false;
            }
        }
    }
}
