﻿using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Stateless;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    [UsedImplicitly]
    public class RecoveryFlowService : IRecoveryFlowService
    {
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;
        private readonly IStateRepository _stateRepository;
        private readonly RecoveryConditions _recoveryConditions;
        private readonly RecoveryContext _ctx;
        private readonly StateMachine<State, Trigger> _stateMachine;

        public RecoveryContext Context => _ctx;

        public RecoveryFlowService(ISmsSender smsSender,
            IEmailSender emailSender,
            IStateRepository stateRepository,
            RecoveryConditions recoveryConditions,
            RecoveryContext ctx)
        {
            _smsSender = smsSender;
            _emailSender = emailSender;
            _stateRepository = stateRepository;
            _recoveryConditions = recoveryConditions;
            _ctx = ctx;
            _stateMachine = new StateMachine<State, Trigger>(ctx.State);
            _stateMachine.OnTransitionedAsync(OnTransitionActionAsync);
            _stateMachine.OnUnhandledTrigger(UnhandledTriggerAction);
            Configure();
        }

        private void UnhandledTriggerAction(State state, Trigger trigger)
        {
            var msg = $"Unable to change state {state} by action {trigger}. Permitted actions {string.Join(',', _stateMachine.GetPermittedTriggers())}";
            throw new InvalidActionException(msg);
        }

        private Task OnTransitionActionAsync(StateMachine<State, Trigger>.Transition transition)
        {
            _ctx.State = transition.Destination;
            _ctx.Time = DateTime.UtcNow;
            _ctx.Action = transition.Trigger;
            _ctx.SeqNo++; // Under any condition SenNo should be incremented only one time per client call

            switch (transition.Destination)
            {
                // For these states Insert will be called only after successful calling of external API
                case State.AwaitSmsVerification:
                case State.AwaitEmailVerification:
                case State.PasswordChangeFrozen:
                case State.AwaitSecretPhrases:
                case State.AwaitDeviceVerification:
                    return Task.CompletedTask;
                default:
                    return _stateRepository.InsertAsync(_ctx);
            }
        }

        public bool IsPasswordUpdateAllowed => _stateMachine.CanFire(Trigger.UpdatePassword);


        private void Configure()
        {
            _stateMachine.Configure(State.RecoveryStarted)
                    .Permit(Trigger.RecoveryRequest, State.AwaitSecretPhrases);

            _stateMachine.Configure(State.AwaitSecretPhrases)
                .PermitSupportStates()
                .OnEntryAsync(OnEntrySecretPhrases)
                .OnExit(OnExitSecretPhrases)
                .Ignore(Trigger.TryUnfreeze)
                .PermitIf(Trigger.SecretPhrasesVerificationFail, State.PasswordChangeForbidden, () => _ctx.SecretPhrasesRecoveryAttempts > _recoveryConditions.SecretPhrasesMaxAttempts)
                .PermitReentryIf(Trigger.SecretPhrasesVerificationFail, () => _ctx.SecretPhrasesRecoveryAttempts <= _recoveryConditions.SecretPhrasesMaxAttempts)
                .Permit(Trigger.JumpToForbidden, State.PasswordChangeForbidden)
                .Permit(Trigger.SecretPhrasesComplete, State.AwaitSmsVerification) // 3 - 6
                .Permit(Trigger.SecretPhrasesSkip, State.AwaitDeviceVerification); // 8 - 28
                                                                                   //
            _stateMachine.Configure(State.AwaitDeviceVerification)
                .PermitSupportStates()
                .OnEntryAsync(OnEntryDeviceVerification)
                .OnExit(OnExitDeviceVerification)
                .Permit(Trigger.DeviceVerificationFail, State.PasswordChangeForbidden)
                .Permit(Trigger.JumpToForbidden, State.PasswordChangeForbidden)
                .Ignore(Trigger.TryUnfreeze)
                .Permit(Trigger.DeviceVerificationComplete, State.AwaitSmsVerification) //For all cases unconditional go to SMS verification
                .Permit(Trigger.DeviceVerificationSkip, State.AwaitSmsVerification);

            _stateMachine.Configure(State.AwaitSmsVerification)
                .OnEntryAsync(SendSmsAsync)
                .PermitSupportStates()
                .Permit(Trigger.JumpToForbidden, State.PasswordChangeForbidden)
                .Ignore(Trigger.TryUnfreeze)
                .Permit(Trigger.SmsVerificationComplete, State.AwaitEmailVerification) // For all cases unconditional go to email verification
                .Permit(Trigger.SmsVerificationSkip, State.AwaitEmailVerification) // For all cases unconditional go to email verification
                .PermitIf(Trigger.SmsVerificationFail, State.PasswordChangeForbidden, () => _ctx.SmsRecoveryAttempts > _recoveryConditions.SmsCodeMaxAttempts)
                .PermitReentryIf(Trigger.SmsVerificationFail, () => _ctx.SmsRecoveryAttempts <= _recoveryConditions.SmsCodeMaxAttempts)
                .PermitIf(Trigger.SmsVerificationRestart, State.PasswordChangeForbidden, () => _ctx.SmsRecoveryAttempts > _recoveryConditions.SmsCodeMaxAttempts)
                .PermitReentryIf(Trigger.SmsVerificationRestart, () => _ctx.SmsRecoveryAttempts <= _recoveryConditions.SmsCodeMaxAttempts);

            _stateMachine.Configure(State.AwaitEmailVerification)
                .Ignore(Trigger.TryUnfreeze)
                .OnEntryAsync(SendEmailAsync)
                .PermitSupportStates()
                .Permit(Trigger.JumpToForbidden, State.PasswordChangeForbidden)
                .PermitIf(Trigger.EmailVerificationComplete, State.PasswordChangeAllowed, () => _ctx.HasSecretPhrases && _ctx.SmsVerified) // 3
                .PermitIf(Trigger.EmailVerificationComplete, State.AwaitSelfieVerification, () => !(_ctx.HasSecretPhrases && _ctx.SmsVerified) && _ctx.KycPassed) // All other cases
                .PermitIf(Trigger.EmailVerificationComplete, State.CallSupport, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && _ctx.SmsVerified ^ true && !_ctx.KycPassed)
                .PermitIf(Trigger.EmailVerificationComplete, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.KycPassed)
                .PermitIf(Trigger.EmailVerificationComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.KycPassed)
                .PermitIf(Trigger.EmailVerificationComplete, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.KycPassed)
                .PermitIf(Trigger.EmailVerificationComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.KycPassed)

                .PermitIf(Trigger.EmailVerificationSkip, State.CallSupport, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && _ctx.SmsVerified ^ false && !_ctx.KycPassed)
                .PermitIf(Trigger.EmailVerificationSkip, State.CallSupport, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && !_ctx.SmsVerified && !_ctx.KycPassed)
                .PermitIf(Trigger.EmailVerificationSkip, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.KycPassed)
                .PermitIf(Trigger.EmailVerificationSkip, State.PasswordChangeForbidden, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.KycPassed)
                .PermitIf(Trigger.EmailVerificationSkip, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.KycPassed)
                .PermitIf(Trigger.EmailVerificationSkip, State.PasswordChangeForbidden, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.KycPassed)
                .PermitIf(Trigger.EmailVerificationSkip, State.AwaitSelfieVerification, () => _ctx.KycPassed) // All cases

                .PermitIf(Trigger.EmailVerificationFail, State.PasswordChangeForbidden, () => _ctx.EmailRecoveryAttempts > _recoveryConditions.EmailCodeMaxAttempts)
                .PermitReentryIf(Trigger.EmailVerificationFail, () => _ctx.EmailRecoveryAttempts <= _recoveryConditions.EmailCodeMaxAttempts)
                .PermitIf(Trigger.EmailVerificationRestart, State.PasswordChangeForbidden, () => _ctx.EmailRecoveryAttempts > _recoveryConditions.EmailCodeMaxAttempts)
                .PermitReentryIf(Trigger.EmailVerificationRestart, () => _ctx.EmailRecoveryAttempts <= _recoveryConditions.EmailCodeMaxAttempts);

            _stateMachine.Configure(State.AwaitSelfieVerification)
                .Ignore(Trigger.TryUnfreeze)
                .PermitSupportStates()
                .Permit(Trigger.JumpToForbidden, State.PasswordChangeForbidden)
                .PermitIf(Trigger.SelfieVerificationSkip, State.CallSupport, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && _ctx.SmsVerified ^ _ctx.EmailVerified) // 4
                .PermitIf(Trigger.SelfieVerificationRequest, State.SelfieVerificationInProgress, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && _ctx.SmsVerified ^ _ctx.EmailVerified) // 5
                .PermitIf(Trigger.SelfieVerificationRequest, State.SelfieVerificationInProgress, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && !_ctx.SmsVerified && !_ctx.EmailVerified) // 6
                .PermitIf(Trigger.SelfieVerificationSkip, State.CallSupport, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && !_ctx.SmsVerified && !_ctx.EmailVerified) // 6.1
                .PermitIf(Trigger.SelfieVerificationRequest, State.SelfieVerificationInProgress, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified) // 8
                .PermitIf(Trigger.SelfieVerificationSkip, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified) // 9, 10
                .PermitIf(Trigger.SelfieVerificationRequest, State.SelfieVerificationInProgress, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified) // 11
                .PermitIf(Trigger.SelfieVerificationSkip, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified) // 12, 13
                .PermitIf(Trigger.SelfieVerificationRequest, State.SelfieVerificationInProgress, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && _ctx.EmailVerified) // 14
                .PermitIf(Trigger.SelfieVerificationSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && _ctx.EmailVerified) // 15
                .PermitIf(Trigger.SelfieVerificationRequest, State.SelfieVerificationInProgress, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.EmailVerified) // 16
                .PermitIf(Trigger.SelfieVerificationSkip, State.PasswordChangeForbidden, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.EmailVerified) // 17
                .PermitIf(Trigger.SelfieVerificationRequest, State.SelfieVerificationInProgress, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified) // 19
                .PermitIf(Trigger.SelfieVerificationSkip, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified) // 20, 21
                .PermitIf(Trigger.SelfieVerificationRequest, State.SelfieVerificationInProgress, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified) // 22
                .PermitIf(Trigger.SelfieVerificationSkip, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified) // 23, 24
                .PermitIf(Trigger.SelfieVerificationRequest, State.SelfieVerificationInProgress, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && _ctx.EmailVerified) // 25
                .PermitIf(Trigger.SelfieVerificationSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && _ctx.EmailVerified) // 26
                .PermitIf(Trigger.SelfieVerificationRequest, State.SelfieVerificationInProgress, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.EmailVerified) // 27
                .PermitIf(Trigger.SelfieVerificationSkip, State.PasswordChangeForbidden, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.EmailVerified); // 28


            _stateMachine.Configure(State.SelfieVerificationInProgress)
                .Ignore(Trigger.TryUnfreeze)
                .PermitSupportStates()
                .Permit(Trigger.JumpToForbidden, State.PasswordChangeForbidden)
                .PermitIf(Trigger.SelfieVerificationComplete, State.PasswordChangeAllowed, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && _ctx.SmsVerified ^ _ctx.EmailVerified)  // 5
                .PermitIf(Trigger.SelfieVerificationComplete, State.CallSupport, () => _ctx.HasSecretPhrases && !_ctx.DeviceVerificationRequested && !_ctx.SmsVerified && !_ctx.EmailVerified)  // 6
                .PermitIf(Trigger.SelfieVerificationComplete, State.PasswordChangeAllowed, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified) // 8
                .PermitIf(Trigger.SelfieVerificationComplete, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified) // 11
                .PermitIf(Trigger.SelfieVerificationComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && _ctx.EmailVerified) // 14
                .PermitIf(Trigger.SelfieVerificationComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.EmailVerified) // 16
                .PermitIf(Trigger.SelfieVerificationComplete, State.Transfer, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified) // 19
                .PermitIf(Trigger.SelfieVerificationComplete, State.AwaitPinCode, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified) // 22
                .PermitIf(Trigger.SelfieVerificationComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && _ctx.EmailVerified) // 25
                .PermitIf(Trigger.SelfieVerificationComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && !_ctx.SmsVerified && !_ctx.EmailVerified) // 27
                .PermitIf(Trigger.SelfieVerificationFail, State.CallSupport); // All other cases go to support

            _stateMachine.Configure(State.AwaitPinCode)
                .Ignore(Trigger.TryUnfreeze)
                .PermitSupportStates()
                .PermitIf(Trigger.PinFail, State.PasswordChangeForbidden, () => _ctx.PinRecoveryAttempts > _recoveryConditions.PinCodeMaxAttempts)
                .PermitReentryIf(Trigger.PinFail, () => _ctx.PinRecoveryAttempts <= _recoveryConditions.PinCodeMaxAttempts)
                .Permit(Trigger.JumpToForbidden, State.PasswordChangeForbidden)
                .PermitIf(Trigger.PinComplete, State.PasswordChangeFrozen, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified && !_ctx.SelfieApproved) // 9
                .PermitIf(Trigger.PinSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified && !_ctx.SelfieApproved) // 10
                .PermitIf(Trigger.PinComplete, State.PasswordChangeFrozen, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && _ctx.SelfieApproved) // 11
                .PermitIf(Trigger.PinComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && !_ctx.SelfieApproved) // 12
                .PermitIf(Trigger.PinSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && _ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && !_ctx.SelfieApproved) // 13
                .PermitIf(Trigger.PinComplete, State.Transfer, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified && !_ctx.SelfieApproved) // 20
                .PermitIf(Trigger.PinSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && _ctx.EmailVerified && !_ctx.SelfieApproved) // 21
                .PermitIf(Trigger.PinComplete, State.Transfer, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && _ctx.SelfieApproved) // 22.1
                .PermitIf(Trigger.PinSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && _ctx.SelfieApproved) // 22.2
                .PermitIf(Trigger.PinComplete, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && !_ctx.SelfieApproved) // 23
                .PermitIf(Trigger.PinSkip, State.CallSupport, () => !_ctx.HasSecretPhrases && !_ctx.DeviceVerified && _ctx.SmsVerified && !_ctx.EmailVerified && !_ctx.SelfieApproved); // 24


            _stateMachine.Configure(State.Transfer)
                .Ignore(Trigger.TryUnfreeze)
                .Permit(Trigger.JumpToForbidden, State.PasswordChangeForbidden)
                .PermitSupportStates();

            _stateMachine.Configure(State.PasswordChangeForbidden)
                .Ignore(Trigger.JumpToForbidden)
                .Ignore(Trigger.TryUnfreeze)
                .PermitSupportStates();


            _stateMachine.Configure(State.PasswordChangeFrozen)
                .OnEntryAsync(Freeze)
                .PermitReentry(Trigger.JumpToFrozen)
                .Permit(Trigger.JumpToForbidden, State.PasswordChangeForbidden)
                .IgnoreIf(Trigger.TryUnfreeze, () => DateTime.UtcNow - (_ctx.FrozenDate ?? DateTime.MaxValue) < TimeSpan.FromDays(_recoveryConditions.FrozenPeriodInDays))
                .PermitIf(Trigger.TryUnfreeze, State.PasswordChangeAllowed, () => DateTime.UtcNow - (_ctx.FrozenDate ?? DateTime.MaxValue) > TimeSpan.FromDays(_recoveryConditions.FrozenPeriodInDays))
                .Permit(Trigger.JumpToAllowed, State.PasswordChangeAllowed)
                .Permit(Trigger.JumpToCallSupport, State.CallSupport)
                .Permit(Trigger.JumpToSuspended, State.PasswordChangeSuspended);

            _stateMachine.Configure(State.PasswordChangeSuspended)
                .Ignore(Trigger.JumpToForbidden)
                .Ignore(Trigger.TryUnfreeze)
                .PermitReentry(Trigger.JumpToSuspended)
                .Permit(Trigger.JumpToAllowed, State.PasswordChangeAllowed)
                .Permit(Trigger.JumpToCallSupport, State.CallSupport)
                .Permit(Trigger.JumpToFrozen, State.PasswordChangeFrozen);
            //            
            _stateMachine.Configure(State.CallSupport)
                .Permit(Trigger.JumpToForbidden, State.PasswordChangeForbidden)
                .Ignore(Trigger.TryUnfreeze)
                .PermitReentry(Trigger.JumpToCallSupport)
                .Permit(Trigger.JumpToAllowed, State.PasswordChangeAllowed)
                .Permit(Trigger.JumpToFrozen, State.PasswordChangeFrozen)
                .Permit(Trigger.JumpToSuspended, State.PasswordChangeSuspended);
            //            
            _stateMachine.Configure(State.PasswordChangeAllowed)
                .Ignore(Trigger.TryUnfreeze)
                .Ignore(Trigger.JumpToForbidden)
                .PermitReentry(Trigger.JumpToAllowed)
                .Permit(Trigger.JumpToCallSupport, State.CallSupport)
                .Permit(Trigger.JumpToFrozen, State.PasswordChangeFrozen)
                .Permit(Trigger.JumpToSuspended, State.PasswordChangeSuspended)
                .Permit(Trigger.UpdatePassword, State.PasswordUpdated);

            _stateMachine.Configure(State.PasswordUpdated)
                .Ignore(Trigger.JumpToForbidden)
                .Ignore(Trigger.TryUnfreeze);
        }

        private void OnExitDeviceVerification()
        {
            _ctx.SignChallengeMessage = null;
        }

        private void OnExitSecretPhrases()
        {
            _ctx.SignChallengeMessage = null;
        }

        private Task OnEntrySecretPhrases()
        {
            _ctx.SignChallengeMessage = Guid.NewGuid().ToString();
            return _stateRepository.InsertAsync(_ctx);
        }

        private Task OnEntryDeviceVerification()
        {
            _ctx.SignChallengeMessage = Guid.NewGuid().ToString();
            return _stateRepository.InsertAsync(_ctx);

        }

        private Task Freeze()
        {
            _ctx.FrozenDate = DateTime.UtcNow;
            return _stateRepository.InsertAsync(_ctx);
        }

        public Task TryUnfreezeAsync()
        {
            return _stateMachine.FireAsync(Trigger.TryUnfreeze);
        }


        private async Task SendSmsAsync()
        {
            await _smsSender.SendCodeAsync(_ctx.ClientId);
            await _stateRepository.InsertAsync(_ctx);
        }

        private async Task SendEmailAsync()
        {
            await _emailSender.SendCodeAsync(_ctx.ClientId);
            await _stateRepository.InsertAsync(_ctx);

        }


        public Task StartRecoveryAsync()
        {
            _ctx.SignChallengeMessage = Guid.NewGuid().ToString();
            return _stateMachine.FireAsync(Trigger.RecoveryRequest);
        }

        public Task SecretPhrasesCompleteAsync()
        {
            _ctx.HasSecretPhrases = true;
            return _stateMachine.FireAsync(Trigger.SecretPhrasesComplete);
        }

        public Task SecretPhrasesSkipAsync()
        {
            _ctx.HasSecretPhrases = false;
            return _stateMachine.FireAsync(Trigger.SecretPhrasesSkip);
        }

        public Task DeviceVerifiedCompleteAsync()
        {
            _ctx.DeviceVerified = true;
            _ctx.DeviceVerificationRequested = true;
            _ctx.SignChallengeMessage = Guid.NewGuid().ToString();
            return _stateMachine.FireAsync(Trigger.DeviceVerificationComplete);
        }

        public Task DeviceVerificationSkipAsync()
        {
            _ctx.DeviceVerified = false;
            _ctx.DeviceVerificationRequested = true;
            return _stateMachine.FireAsync(Trigger.DeviceVerificationSkip);
        }

        public Task SmsVerificationCompleteAsync()
        {
            _ctx.SmsVerified = true;
            return _stateMachine.FireAsync(Trigger.SmsVerificationComplete);
        }

        public Task SmsVerificationSkipAsync()
        {
            _ctx.SmsVerified = false;
            return _stateMachine.FireAsync(Trigger.SmsVerificationSkip);
        }

        public Task SmsVerificationRestartAsync()
        {
            _ctx.SmsRecoveryAttempts++;
            _ctx.SmsVerified = false;
            return _stateMachine.FireAsync(Trigger.SmsVerificationRestart);
        }

        public Task SmsVerificationFailedAsync()
        {
            _ctx.SmsRecoveryAttempts++;
            _ctx.SmsVerified = false;
            return _stateMachine.FireAsync(Trigger.SmsVerificationFail);
        }

        public Task EmailVerificationCompleteAsync()
        {
            _ctx.EmailVerified = true;
            return _stateMachine.FireAsync(Trigger.EmailVerificationComplete);
        }

        public Task UpdatePasswordCompleteAsync()
        {
            return _stateMachine.FireAsync(Trigger.UpdatePassword);
        }

        public Task EmailVerificationSkipAsync()
        {
            _ctx.EmailVerified = false;
            return _stateMachine.FireAsync(Trigger.EmailVerificationSkip);
        }

        public Task EmailVerificationRestartAsync()
        {
            _ctx.EmailRecoveryAttempts++;
            _ctx.EmailVerified = false;
            return _stateMachine.FireAsync(Trigger.EmailVerificationRestart);
        }

        public Task EmailVerificationFailedAsync()
        {
            _ctx.EmailRecoveryAttempts++;
            _ctx.EmailVerified = false;
            return _stateMachine.FireAsync(Trigger.EmailVerificationFail);
        }

        public Task SelfieVerificationRequestAsync()
        {
            _ctx.SelfieApproved = false;
            return _stateMachine.FireAsync(Trigger.SelfieVerificationRequest);
        }

        public Task SelfieVerificationSkipAsync()
        {
            _ctx.SelfieApproved = false;
            return _stateMachine.FireAsync(Trigger.SelfieVerificationSkip);
        }

        public Task SelfieVerificationFailAsync()
        {
            _ctx.SelfieApproved = false;
            return _stateMachine.FireAsync(Trigger.SelfieVerificationFail);
        }

        public Task SelfieVerificationCompleteAsync()
        {
            _ctx.SelfieApproved = true;
            return _stateMachine.FireAsync(Trigger.SelfieVerificationComplete);
        }

        public Task PinCodeVerificationCompleteAsync()
        {
            _ctx.HasPin = true;
            return _stateMachine.FireAsync(Trigger.PinComplete);
        }

        public Task PinCodeVerificationSkipAsync()
        {
            _ctx.HasPin = false;
            return _stateMachine.FireAsync(Trigger.PinSkip);
        }

        public Task PinCodeVerificationFailAsync()
        {
            _ctx.PinRecoveryAttempts++;
            _ctx.HasPin = false;
            return _stateMachine.FireAsync(Trigger.PinFail);
        }

        public Task JumpToForbiddenAsync()
        {
            return _stateMachine.FireAsync(Trigger.JumpToForbidden);
        }

        public Task SecretPhrasesVerificationFailAsync()
        {
            _ctx.SecretPhrasesRecoveryAttempts++;
            return _stateMachine.FireAsync(Trigger.SecretPhrasesVerificationFail);
        }

        public Task DeviceVerificationFailAsync()
        {
            return _stateMachine.FireAsync(Trigger.DeviceVerificationFail);
        }

        #region Only for support

        public Task JumpToAllowAsync()
        {
            return _stateMachine.FireAsync(Trigger.JumpToAllowed);
        }

        public Task JumpToSupportAsync()
        {
            return _stateMachine.FireAsync(Trigger.JumpToCallSupport);
        }




        public Task JumpToSuspendAsync()
        {
            return _stateMachine.FireAsync(Trigger.JumpToSuspended);
        }
        #endregion


    }

    internal static class StateMachineEx
    {
        public static StateMachine<State, Trigger>.StateConfiguration PermitSupportStates(this StateMachine<State, Trigger>.StateConfiguration stateMachine)
        {
            var result = stateMachine
                .Permit(Trigger.JumpToAllowed, State.PasswordChangeAllowed)
                .Permit(Trigger.JumpToCallSupport, State.CallSupport)
                .Permit(Trigger.JumpToFrozen, State.PasswordChangeFrozen)
                .Permit(Trigger.JumpToSuspended, State.PasswordChangeSuspended);
            return result;
        }
    }
}
