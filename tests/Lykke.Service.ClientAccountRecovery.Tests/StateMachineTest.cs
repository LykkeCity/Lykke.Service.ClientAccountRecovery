using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ClientAccountRecovery.Services;
using NSubstitute;
using NUnit.Framework;

namespace Lykke.Service.ClientAccountRecovery.Tests
{
    [TestFixture]
    public class StateMachineTest
    {
        private RecoveryFlowService _flowService;
        private ISmsSender _smsSender;
        private IEmailSender _emailSender;
        private IStateRepository _stateRepository;
        private RecoveryContext _attr;
        private RecoveryConditions _recoveryConditions;

        [SetUp]
        public void SetUp()
        {
            _recoveryConditions = new RecoveryConditions
            {
                EmailCodeMaxAttempts = 3,
                SmsCodeMaxAttempts = 3,
                FrozenPeriodInDays = 5
            };
            _smsSender = Substitute.For<ISmsSender>();
            _emailSender = Substitute.For<IEmailSender>();
            _stateRepository = Substitute.For<IStateRepository>();
            _attr = new RecoveryContext { State = State.RecoveryStarted };
            _flowService = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, _attr);
        }


        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeAllowed, 3)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeAllowed, 3.5)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 4.1)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 4.15)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 4.2)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeAllowed, 5.1)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 5.15)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ignore, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 4.2)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ignore, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeAllowed, 5.1)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ignore, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 5.15)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeAllowed, 5.2)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 5.25)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 5.3)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 5.3)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 6.1)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 6.15)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 6.2)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 6.3)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ignore, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 6.1)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ignore, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 6.15)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ignore, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 6.2)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Ignore, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 6.3)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeAllowed, 8.1)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 8.2)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeFrozen, 9.1)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeFrozen, 9.15)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 10)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.Yes, State.CallSupport, 10)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 10.5)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.Yes, State.CallSupport, 10.5)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeFrozen, 11.1)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 11.2)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 12)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 12.5)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 13)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.Yes, State.CallSupport, 13)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 13.5)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.Yes, State.CallSupport, 13.5)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 14.1)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 14.2)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 15)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 15.5)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 16.1)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 16.2)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeForbidden, 17)]
        [TestCase(Words.Skip, Device.Ok, Sms.Skip, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeForbidden, 17.5)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ignore, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 14.1)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ignore, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 14.2)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ignore, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 15)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ignore, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 15.5)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ignore, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 16.1)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ignore, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 16.2)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ignore, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeForbidden, 17)]
        [TestCase(Words.Skip, Device.Ok, Sms.Ignore, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeForbidden, 17.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.Transfer, 19.1)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 19.2)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.Transfer, 20)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.Transfer, 20.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 21)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.Yes, State.CallSupport, 21)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 21.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.Yes, State.CallSupport, 21.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.Transfer, 22.1)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 22.2)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.Yes, State.CallSupport, 22.2)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 22.3)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 23)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 23.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 24)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.Yes, State.CallSupport, 24)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 24.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.Yes, State.CallSupport, 24.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 25.1)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 25.2)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 26)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 26.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 27.1)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 27.2)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeForbidden, 28)]
        [TestCase(Words.Skip, Device.Skip, Sms.Skip, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeForbidden, 28.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ignore, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 25.1)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ignore, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 25.2)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ignore, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 26)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ignore, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 26.5)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ignore, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 27.1)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ignore, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.CallSupport, 27.2)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ignore, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeForbidden, 28)]
        [TestCase(Words.Skip, Device.Skip, Sms.Ignore, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.Yes, State.PasswordChangeForbidden, 28.5)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.Transfer, 19.1)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 19.2)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.Transfer, 20)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.Transfer, 20.5)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 21)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.No, State.CallSupport, 21)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 21.5)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.No, State.CallSupport, 21.5)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.Transfer, 22.1)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 22.2)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.No, State.CallSupport, 22.2)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 22.3)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 23)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ok, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 23.5)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 24)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.No, State.CallSupport, 24)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Skip, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 24.5)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ok, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.No, PublicKeyKnown.No, State.CallSupport, 24.5)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 25.1)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 25.2)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Skip, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 26)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Skip, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 26.5)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 27.1)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 27.2)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Skip, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.PasswordChangeForbidden, 28)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Skip, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.Yes, PinKnown.Yes, PublicKeyKnown.No, State.PasswordChangeForbidden, 28.5)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ignore, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 25.1)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ignore, Email.Ok, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 25.2)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ignore, Email.Ok, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 26)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ignore, Email.Ok, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 26.5)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ignore, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Ok, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 27.1)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ignore, Email.Skip, KycPassed.Ok, Selfie.Ok, SelfieCheck.Fail, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.No, State.CallSupport, 27.2)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ignore, Email.Skip, KycPassed.Ok, Selfie.Skip, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.No, State.PasswordChangeForbidden, 28)]
        [TestCase(Words.Ignore, Device.Ignore, Sms.Ignore, Email.Skip, KycPassed.Fail, Selfie.Ignore, SelfieCheck.Ignore, Pin.Ignore, Phone.No, PinKnown.Yes, PublicKeyKnown.No, State.PasswordChangeForbidden, 28.5)]
        public async Task StateMachine_WhenCaseApplied_FinalStateIsCorrect(Ss secretPhrases, Ss deviceVerified, Ss sms,
            Ss email, Ss kycPassed, Ss selfieReceived, Ss selfiePass, Ss pin, Phone phone, PinKnown pinKnown, PublicKeyKnown publicKeyKnown,
            State resolution, double line)
        {
            _flowService.Context.PinKnown = pinKnown == PinKnown.Yes;
            _flowService.Context.KycPassed = kycPassed == Ss.Ok;
            _flowService.Context.HasPhoneNumber = phone == Phone.Yes;
            _flowService.Context.PublicKeyKnown = publicKeyKnown == PublicKeyKnown.Yes;
            await _flowService.StartRecoveryAsync();
            await Fire(secretPhrases, _flowService.SecretPhrasesCompleteAsync, _flowService.SecretPhrasesSkipAsync);
            await Fire(deviceVerified, _flowService.DeviceVerifiedCompleteAsync, _flowService.DeviceVerificationSkipAsync);
            await Fire(sms, _flowService.SmsVerificationCompleteAsync, _flowService.SmsVerificationSkipAsync, _flowService.SmsVerificationFailedAsync, _flowService.SmsVerificationRestartAsync);
            await Fire(email, _flowService.EmailVerificationCompleteAsync, _flowService.EmailVerificationSkipAsync, _flowService.EmailVerificationFailedAsync, _flowService.EmailVerificationRestartAsync);
            await Fire(selfieReceived, _flowService.SelfieVerificationRequestAsync, _flowService.SelfieVerificationSkipAsync);
            await Fire(selfiePass, _flowService.SelfieVerificationCompleteAsync, null, _flowService.SelfieVerificationFailAsync);
            await Fire(pin, _flowService.PinCodeVerificationCompleteAsync, _flowService.PinCodeVerificationSkipAsync);

            if (resolution != _flowService.Context.State)
            {
                Assert.True(false, $"Expected {resolution} but was {_flowService.Context.State} for line {line}");
            }
        }

        [Test]
        [TestCase(State.PasswordChangeAllowed)]
        [TestCase(State.CallSupport)]
        [TestCase(State.PasswordChangeSuspended)]
        public async Task StateMachine_Allows_Switch_Support_States(State supportState)
        {
            var allStates = Enum.GetValues(typeof(State)).Cast<State>().Except(new[] { supportState, State.RecoveryStarted, State.PasswordUpdated });
            foreach (var state in allStates)
            {
                var context = new RecoveryContext
                {
                    State = state
                };
                var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);
                switch (supportState)
                {
                    case State.PasswordChangeAllowed:
                        await stateMachine.JumpToAllowAsync();
                        break;
                    case State.CallSupport:
                        await stateMachine.JumpToSupportAsync();
                        break;
                    case State.PasswordChangeSuspended:
                        await stateMachine.JumpToSuspendAsync();
                        break;
                }
                Assert.That(supportState, Is.EqualTo(stateMachine.Context.State));
            }
        }

        [Test]
        public async Task ShouldAllowToGoFinalState()
        {
            var context = new RecoveryContext
            {
                State = State.PasswordChangeAllowed
            };
            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.UpdatePasswordCompleteAsync();

            Assert.That(State.PasswordUpdated, Is.EqualTo(stateMachine.Context.State));
        }

        [Test]
        public void ShouldSealState()
        {
            var context = new RecoveryContext
            {
                State = State.PasswordUpdated
            };
            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);



            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.UpdatePasswordCompleteAsync());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToSuspendAsync());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToAllowAsync());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.JumpToSupportAsync());
            Assert.ThrowsAsync<InvalidActionException>(() => stateMachine.StartRecoveryAsync());
        }

        [Test]
        public async Task Should_Set_Forbidden([Values]State initialState)
        {
            if (initialState == State.RecoveryStarted)
            {
                Assert.Pass();
            }
            var ignoredStates = new[]
            {
                State.PasswordChangeAllowed,
                State.PasswordUpdated,
                State.PasswordChangeSuspended,
                State.PasswordChangeForbidden,
            };
            var context = new RecoveryContext
            {
                State = initialState
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.JumpToForbiddenAsync();

            if (ignoredStates.Contains(initialState))
            {
                Assert.That(stateMachine.Context.State, Is.EqualTo(initialState));
            }
            else
            {
                Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeForbidden));

            }
        }


        [Test]
        public async Task ShouldSendSmsRequest()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitDeviceVerification,
                HasPhoneNumber = true
            };
            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);
            await stateMachine.DeviceVerifiedCompleteAsync();

            await _smsSender.ReceivedWithAnyArgs().SendCodeAsync("1");
        }
        //
        [Test]
        public async Task ShouldUnfreezeAfterPeriod()
        {
            var context = new RecoveryContext
            {
                HasSecretPhrases = false,
                DeviceVerified = true,
                DeviceVerificationRequested = true,
                SmsVerified = true,
                EmailVerified = true,
                SelfieApproved = false,
                State = State.AwaitPinCode
            };
            _recoveryConditions.FrozenPeriodInDays = TimeSpan.FromSeconds(1).TotalDays;

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);
            await stateMachine.PinCodeVerificationCompleteAsync();


            await Task.Delay(TimeSpan.FromSeconds(2));
            await stateMachine.TryUnfreezeAsync();

            Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeAllowed));
        }

        [Test]
        public async Task ShouldKeepFrozen()
        {
            var context = new RecoveryContext
            {
                HasSecretPhrases = false,
                DeviceVerified = true,
                DeviceVerificationRequested = true,
                SmsVerified = true,
                EmailVerified = true,
                SelfieApproved = false,
                State = State.AwaitPinCode
            };
            _recoveryConditions.FrozenPeriodInDays = TimeSpan.FromSeconds(2).TotalDays;

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);
            await stateMachine.PinCodeVerificationCompleteAsync();

            await Task.Delay(TimeSpan.FromSeconds(1));
            await stateMachine.TryUnfreezeAsync();

            Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeFrozen));
        }


        [Test]
        public void ShouldIgnoreFreezeTrigger()
        {
            var allStates = Enum.GetValues(typeof(State)).Cast<State>().Except(new[]
            {
                State.RecoveryStarted, State.PasswordChangeFrozen, State.PasswordChangeAllowed, State.PasswordUpdated
            });
            foreach (var state in allStates)
            {
                var context = new RecoveryContext
                {
                    State = state
                };
                var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

                Assert.That(state, Is.EqualTo(stateMachine.Context.State));
            }
        }

        [Test]
        public async Task Should_BlockRecovery_AfterMaxSmsAttempts()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitSmsVerification
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.SmsVerificationFailedAsync();
            await stateMachine.SmsVerificationFailedAsync();
            await stateMachine.SmsVerificationFailedAsync();

            Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeForbidden));
        }

        [Test]
        public async Task Should_BlockRecovery_AfterMaxSmsRestarts()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitSmsVerification
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.SmsVerificationRestartAsync();
            await stateMachine.SmsVerificationRestartAsync();
            await stateMachine.SmsVerificationRestartAsync();

            Assert.That(stateMachine.Context.State, Is.EqualTo(State.AwaitEmailVerification));
        }

        [Test]
        public async Task Should_BlockRecovery_AfterMaxEmailsAttempts()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitEmailVerification
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.EmailVerificationFailedAsync();
            await stateMachine.EmailVerificationFailedAsync();
            await stateMachine.EmailVerificationFailedAsync();

            Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeForbidden));
        }

        [TestCase(Words.Ok, Device.Ignore, Sms.Ok, KycPassed.Fail, PinKnown.No, ExpectedResult = State.CallSupport)]
        [TestCase(Words.Ok, Device.Ignore, Sms.Fail, KycPassed.Fail, PinKnown.No, ExpectedResult = State.CallSupport)]
        [TestCase(Words.Fail, Device.Ok, Sms.Ok, KycPassed.Fail, PinKnown.Yes, ExpectedResult = State.AwaitPinCode)]
        [TestCase(Words.Fail, Device.Ok, Sms.Ok, KycPassed.Fail, PinKnown.No, ExpectedResult = State.CallSupport)]
        [TestCase(Words.Fail, Device.Ok, Sms.Fail, KycPassed.Fail, PinKnown.No, ExpectedResult = State.PasswordChangeForbidden)]
        [TestCase(Words.Fail, Device.Fail, Sms.Ok, KycPassed.Fail, PinKnown.Yes, ExpectedResult = State.AwaitPinCode)]
        [TestCase(Words.Fail, Device.Fail, Sms.Ok, KycPassed.Fail, PinKnown.No, ExpectedResult = State.CallSupport)]
        [TestCase(Words.Fail, Device.Fail, Sms.Fail, KycPassed.Fail, PinKnown.No, ExpectedResult = State.PasswordChangeForbidden)]
        [TestCase(Words.Fail, Device.Fail, Sms.Fail, KycPassed.Ok, PinKnown.No, ExpectedResult = State.AwaitSelfieVerification)]
        public async Task<State> Should_BlockRecovery_AfterMaxEmailsRestarts(Ss words, Ss device, Ss sms, Ss kyc, PinKnown pin)
        {
            var context = new RecoveryContext
            {
                HasSecretPhrases = words == Ss.Ok,
                DeviceVerificationRequested = device != Ss.Ignore,
                DeviceVerified = device == Ss.Ok,
                SmsVerified = sms == Ss.Ok,
                State = State.AwaitEmailVerification,
                PinKnown = pin == PinKnown.Yes,
                KycPassed = kyc == Ss.Ok
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.EmailVerificationRestartAsync();
            await stateMachine.EmailVerificationRestartAsync();
            await stateMachine.EmailVerificationRestartAsync();

            return stateMachine.Context.State;
        }


        [TestCase(1, State.AwaitSecretPhrases)]
        [TestCase(2, State.AwaitSecretPhrases)]
        [TestCase(3, State.PasswordChangeForbidden)]
        public async Task Should_BlockRecovery_AfterMaxSecretPhrasesAttempts(int attemptsNo, State expectedState)
        {
            var context = new RecoveryContext
            {
                State = State.AwaitSecretPhrases
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            for (int i = 0; i < attemptsNo; i++)
            {
                await stateMachine.SecretPhrasesVerificationFailAsync();
            }


            Assert.That(stateMachine.Context.State, Is.EqualTo(expectedState));
        }

        [TestCase(1, State.AwaitPinCode)]
        [TestCase(2, State.AwaitPinCode)]
        [TestCase(3, State.PasswordChangeForbidden)]
        public async Task Should_BlockRecovery_AfterMaxPinAttempts(int attemptsNo, State expectedState)
        {
            var context = new RecoveryContext
            {
                State = State.AwaitPinCode
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            for (int i = 0; i < attemptsNo; i++)
            {
                await stateMachine.PinCodeVerificationFailAsync();
            }

            Assert.That(stateMachine.Context.State, Is.EqualTo(expectedState));
        }

        [Test]
        public async Task Should_BlockRecovery_AfterFirstFailedDeviceRecoveryAttempts()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitDeviceVerification
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);

            await stateMachine.DeviceVerificationFailAsync();

            Assert.That(stateMachine.Context.State, Is.EqualTo(State.PasswordChangeForbidden));
        }

        [Test]
        public void Allows_PasswordRecovery_OnlyInTerminalState([Values] State state)
        {
            var context = new RecoveryContext
            {
                State = state
            };

            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);


            if (state == State.PasswordChangeAllowed)
            {
                Assert.That(stateMachine.IsPasswordUpdateAllowed, Is.True);
            }
            else
            {
                Assert.That(stateMachine.IsPasswordUpdateAllowed, Is.False);
            }
        }

        [Test]
        public async Task ShouldSendEmailRequest()
        {
            var context = new RecoveryContext
            {
                State = State.AwaitSmsVerification
            };
            var stateMachine = new RecoveryFlowService(_smsSender, _emailSender, _stateRepository, _recoveryConditions, context);
            await stateMachine.SmsVerificationCompleteAsync();

            await _emailSender.ReceivedWithAnyArgs().SendCodeAsync("1");
        }

        [Test, Explicit("Only for manual graph generating")]
        public void GetStateMachineGraph()
        {
            var grpaph = _flowService.GetGraph();
        }

        private Task Fire(Ss state, Func<Task> ok, Func<Task> skip, Func<Task> fail = null, Func<Task> restart = null)
        {
            Debug.Assert(ok != skip);
            switch (state)
            {
                case Ss.Ok:
                    return ok();
                case Ss.Skip:
                    if (skip == null)
                    {
                        throw new ArgumentNullException(nameof(skip));
                    }
                    return skip();
                case Ss.Fail:
                    if (fail == null)
                    {
                        throw new ArgumentNullException(nameof(fail));
                    }
                    return fail();
                case Ss.Restart:
                    if (restart == null)
                    {
                        throw new ArgumentNullException(nameof(restart));
                    }
                    return restart();
            }
            return Task.CompletedTask;
        }



        public enum Ss // Step state for short
        {

            Ignore = 0,
            Ok = 1,
            Skip = 2,
            Fail = 3,
            Restart = 4
        }

        private enum Words // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum Device // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum Sms // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum Email // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum Selfie // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum SelfieCheck // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum Pin // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        private enum KycPassed // Step state for short
        {
            Ignore,
            Ok,
            Skip,
            Fail,
            Restart
        }

        public enum Phone // Step state for short
        {
            Yes = 1,
            No = 3
        }

        public enum PinKnown // Step state for short
        {
            Yes = 1,
            No = 3
        }

        public enum PublicKeyKnown // Step state for short
        {
            Yes = 1,
            No = 3
        }
    }

}
