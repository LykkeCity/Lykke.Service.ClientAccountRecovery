using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Action = Lykke.Service.ClientAccountRecovery.Core.Domain.Action;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class ChallengeManager : IChallengeManager
    {
        private readonly IChallengeValidatorFactory _challengeValidatorFactory;
        private readonly ISelfieNotificationSender _selfieNotificationSender;

        public ChallengeManager(IChallengeValidatorFactory challengeValidatorFactory, ISelfieNotificationSender selfieNotificationSender)
        {
            _challengeValidatorFactory = challengeValidatorFactory;
            _selfieNotificationSender = selfieNotificationSender;
        }

        public async Task<bool> ExecuteAction(Challenge challenge, Action action, string code, IRecoveryFlowService flow)
        {
            var switcher = (challenge, action);
            switch (switcher)
            {
                case var a when a.Item1 == Challenge.Words && a.Item2 == Action.Complete:
                    return await Validate(Challenge.Words, flow, code);
                case var a when a.Item1 == Challenge.Words && a.Item2 == Action.Skip:
                    await flow.SecretPhrasesSkipAsync();
                    return true;
                case var a when a.Item1 == Challenge.Device && a.Item2 == Action.Complete:
                    return await Validate(Challenge.Device, flow, code);
                case var a when a.Item1 == Challenge.Device && a.Item2 == Action.Skip:
                    await flow.DeviceVerificationSkipAsync();
                    return true;
                case var a when a.Item1 == Challenge.Sms && a.Item2 == Action.Complete:
                    return await Validate(Challenge.Sms, flow, code);
                case var a when a.Item1 == Challenge.Sms && a.Item2 == Action.Skip:
                    await flow.SmsVerificationSkipAsync();
                    return true;
                case var a when a.Item1 == Challenge.Sms && a.Item2 == Action.Restart:
                    await flow.SmsVerificationRestartAsync();
                    return true;
                case var a when a.Item1 == Challenge.Email && a.Item2 == Action.Complete:
                    return await Validate(Challenge.Email, flow, code);
                case var a when a.Item1 == Challenge.Email && a.Item2 == Action.Skip:
                    await flow.EmailVerificationSkipAsync();
                    return true;
                case var a when a.Item1 == Challenge.Email && a.Item2 == Action.Restart:
                    await flow.EmailVerificationRestartAsync();
                    return true;
                case var a when a.Item1 == Challenge.Selfie && a.Item2 == Action.Complete:
                    await NotifySelfiePosted(flow, code);
                    return true;
                case var a when a.Item1 == Challenge.Selfie && a.Item2 == Action.Skip:
                    await flow.SelfieVerificationSkipAsync();
                    return true;
                case var a when a.Item1 == Challenge.Pin && a.Item2 == Action.Complete:
                    return await Validate(Challenge.Pin, flow, code);
                case var a when a.Item1 == Challenge.Pin && a.Item2 == Action.Skip:
                    await flow.PinCodeVerificationSkipAsync();
                    return true;
            }

            throw new ArgumentException($"Invalid pair {challenge} {action}");
        }

        private Task<bool> Validate(Challenge challenge, IRecoveryFlowService flow, string code)
        {
            return _challengeValidatorFactory.GetValidator(challenge).Confirm(flow, code);
        }

        private Task NotifySelfiePosted(IRecoveryFlowService flow, string code)
        {
            return _selfieNotificationSender.Send(flow, code);
        }
    }
}
