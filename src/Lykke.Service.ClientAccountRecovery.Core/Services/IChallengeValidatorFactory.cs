using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    public interface IChallengeValidatorFactory
    {
        IChallengesValidator GetValidator(Challenge challenge);
    }
}
