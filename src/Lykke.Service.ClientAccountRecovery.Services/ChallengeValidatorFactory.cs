using System;
using Autofac;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class ChallengeValidatorFactory : IChallengeValidatorFactory
    {
        private readonly ILifetimeScope _container;

        public ChallengeValidatorFactory(ILifetimeScope container)
        {
            _container = container;
        }

        public IChallengesValidator GetValidator(Challenge challenge)
        {
            IChallengesValidator result;
            switch (challenge)
            {
                case Challenge.Sms:
                    result = _container.Resolve<SmsValidator>();
                    break;
                case Challenge.Email:
                    result = _container.Resolve<EmailValidator>();
                    break;
                case Challenge.Words:
                    result = _container.Resolve<SecretPhrasesValidator>();
                    break;
                case Challenge.Device:
                    result = _container.Resolve<DeviceValidator>();
                    break;
                case Challenge.Pin:
                    result = _container.Resolve<PinValidator>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(challenge), challenge, null);
            }

            return result;
        }
    }
}
