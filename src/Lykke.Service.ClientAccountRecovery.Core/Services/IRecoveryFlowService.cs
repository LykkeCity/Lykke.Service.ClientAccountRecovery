using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    public interface IRecoveryFlowService
    {
        RecoveryContext Context { get; }
        bool IsPasswordUpdateAllowed { get; }

        Task StartRecoveryAsync();
        Task SecretPhrasesCompleteAsync();
        Task SecretPhrasesSkipAsync();
        Task DeviceVerifiedCompleteAsync();
        Task DeviceVerificationSkipAsync();
        Task SmsVerificationCompleteAsync();
        Task SmsVerificationSkipAsync();
        Task SmsVerificationRestartAsync();
        Task SmsVerificationFailedAsync();
        Task EmailVerificationCompleteAsync();
        Task EmailVerificationSkipAsync();
        Task EmailVerificationRestartAsync();
        Task EmailVerificationFailedAsync();
        Task SelfieVerificationRequestAsync();
        Task SelfieVerificationSkipAsync();
        Task SelfieVerificationFailAsync();
        Task SelfieVerificationCompleteAsync();
        Task PinCodeVerificationCompleteAsync();
        Task PinCodeVerificationSkipAsync();
        Task PinCodeVerificationFailAsync();
        Task JumpToAllowAsync();
        Task JumpToSupportAsync();
        Task JumpToSuspendAsync();
        Task UpdatePasswordCompleteAsync();
        Task TryUnfreezeAsync();
        Task JumpToForbiddenAsync();
        Task SecretPhrasesVerificationFailAsync();
        Task DeviceVerificationFailAsync();
    }
}
