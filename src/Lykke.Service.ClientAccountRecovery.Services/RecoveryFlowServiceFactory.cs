using System;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.SettingsReader;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class RecoveryFlowServiceFactory : IRecoveryFlowServiceFactory
    {
        private readonly ILifetimeScope _container;
        private readonly IRecoveryLogRepository _repository;
        private readonly IKycStatusService _kycStatusService;
        private readonly IClientAccountClient _accountClient;
        private readonly IWalletCredentialsRepository _credentialsRepository;


        public RecoveryFlowServiceFactory(ILifetimeScope container,
            IRecoveryLogRepository repository,
            IKycStatusService kycStatusService,
            IClientAccountClient accountClient,
            IWalletCredentialsRepository credentialsRepository)
        {
            _container = container;
            _repository = repository;
            _kycStatusService = kycStatusService;
            _accountClient = accountClient;
            _credentialsRepository = credentialsRepository;
        }

        public async Task<IRecoveryFlowService> InitiateNew(string clientId)
        {
            var initialContext = new RecoveryContext
            {
                RecoveryId = Guid.NewGuid().ToString(),
                ClientId = clientId,
                KycPassed = await IsKycPassed(clientId),
                HasPhoneNumber = await ClientHasPhoneNumber(clientId),
                PinKnown = await IsPinEntered(clientId),
                PublicKeyKnown = await PublicKeyKnown(clientId)
            };
            var recoveryConditions = _container.Resolve<IReloadingManager<RecoveryConditions>>().CurrentValue;
            var service = _container.Resolve<IRecoveryFlowService>(
                TypedParameter.From(initialContext),
                TypedParameter.From(recoveryConditions));
            return service;
        }

        public async Task<IRecoveryFlowService> FindExisted(string recoveryId)
        {
            var log = await _repository.GetAsync(recoveryId);
            if (log.Empty)
            {
                return null;
            }
            var context = log.ActualStatus;
            context.KycPassed = await IsKycPassed(context.ClientId);
            context.HasPhoneNumber = await ClientHasPhoneNumber(context.ClientId);
            context.PinKnown = await IsPinEntered(context.ClientId);
            context.PublicKeyKnown = await PublicKeyKnown(context.ClientId);

            var recoveryConditions = _container.Resolve<IReloadingManager<RecoveryConditions>>().CurrentValue;

            var service = _container.Resolve<IRecoveryFlowService>(
                TypedParameter.From(context),
                TypedParameter.From(recoveryConditions));
            return service;
        }


        private async Task<bool> IsKycPassed(string clientId)
        {
            var status = await _kycStatusService.GetKycStatusAsync(clientId);
            return status == KycStatus.Ok;
        }

        private async Task<bool> ClientHasPhoneNumber(string clientId)
        {
            var clientModel = await _accountClient.GetByIdAsync(clientId);
            if (clientModel == null)
            {
                throw new InvalidOperationException($"The inconsistent state. Unable to find a client with id {clientId}");
            }
            return !string.IsNullOrWhiteSpace(clientModel.Phone);
        }

        private Task<bool> IsPinEntered(string clientId)
        {
            return _accountClient.IsPinEnteredAsync(clientId);
        }

        private async Task<bool> PublicKeyKnown(string clientId)
        {
            return (await _credentialsRepository.GetAsync(clientId))?.Address != null;
        }
    }
}
