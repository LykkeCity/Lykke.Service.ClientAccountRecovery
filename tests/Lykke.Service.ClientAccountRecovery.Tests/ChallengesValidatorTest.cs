using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ClientAccountRecovery.Services;
using Lykke.Service.ConfirmationCodes.Client;
using Lykke.Service.ConfirmationCodes.Client.Models.Response;
using NBitcoin;
using NSubstitute;
using NUnit.Framework;

namespace Lykke.Service.ClientAccountRecovery.Tests
{
    [TestFixture]
    public class ChallengesValidatorTest
    {
        private const string PublicAddress = "mhsp2ukHTfXfu4ixrQEwULNcduFkK5qnPW";
        private const string PrivateKey = "KwDiBf89QgGbjEhKnhXJuH7Ro98XrkJMQ5PFYQ4reorxqRmVKhvL";
        private IConfirmationCodesClient _confirmationCodesClient;
        private IClientAccountClient _clientAccountClient;
        private IWalletCredentialsRepository _credentialsRepository;
        private SecretPhrasesValidator _phrasesValidator;
        private DeviceValidator _deviceValidator;
        private PinValidator _pinValidator;
        private SmsValidator _smsValidator;
        private EmailValidator _emailValidator;


        [SetUp]
        public void SetUp()
        {
            _confirmationCodesClient = Substitute.For<IConfirmationCodesClient>();
            _clientAccountClient = Substitute.For<IClientAccountClient>();
            _credentialsRepository = Substitute.For<IWalletCredentialsRepository>();
            _phrasesValidator = new SecretPhrasesValidator(_credentialsRepository, LogFactory.Create());
            _deviceValidator = new DeviceValidator(_credentialsRepository, LogFactory.Create());
            _pinValidator = new PinValidator(_clientAccountClient);
            _smsValidator = new SmsValidator(_confirmationCodesClient, _clientAccountClient);
            _emailValidator = new EmailValidator(_confirmationCodesClient, _clientAccountClient);
        }

        [TestCase("Correct code", true)]
        [TestCase("Invalid code", false)]
        public async Task Should_CorrectlyValidate_SecretPhrases(string code, bool isValid)
        {
            const string challenge = "Correct code";
            var clientID = Guid.NewGuid().ToString();
            var key = Key.Parse(PrivateKey);

            var signature = key.SignMessage(code);


            var flow = Substitute.For<IRecoveryFlowService>();
            var context = new RecoveryContext
            {
                ClientId = clientID,
                SignChallengeMessage = challenge
            };
            flow.Context.Returns(context);

            var credentials = Substitute.For<IWalletCredentials>();
            credentials.Address.Returns(PublicAddress);
            _credentialsRepository.GetAsync(clientID).Returns(Task.FromResult(credentials));

            await _phrasesValidator.Confirm(flow, signature);

            if (isValid)
            {
                await flow.Received().SecretPhrasesCompleteAsync();
                await flow.DidNotReceive().SecretPhrasesVerificationFailAsync();
            }
            else
            {
                await flow.DidNotReceive().SecretPhrasesCompleteAsync();
                await flow.Received().SecretPhrasesVerificationFailAsync();
            }

        }


        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_CorrectlyValidate_Sms(bool isValid)
        {
            var clientID = Guid.NewGuid().ToString();


            var flow = Substitute.For<IRecoveryFlowService>();
            var context = new RecoveryContext
            {
                ClientId = clientID
            };

            flow.Context.Returns(context);
            _clientAccountClient.GetByIdAsync("").ReturnsForAnyArgs(new ClientModel());

            _confirmationCodesClient.VerifySmsCodeAsync(null).ReturnsForAnyArgs(new VerificationResult { IsValid = isValid });

            await _smsValidator.Confirm(flow, "SomeCode");

            if (isValid)
            {
                await flow.Received().SmsVerificationCompleteAsync();
                await flow.DidNotReceive().SmsVerificationFailedAsync();
            }
            else
            {
                await flow.DidNotReceive().SmsVerificationCompleteAsync();
                await flow.Received().SmsVerificationFailedAsync();
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_CorrectlyValidate_Email(bool isValid)
        {
            var clientID = Guid.NewGuid().ToString();


            var flow = Substitute.For<IRecoveryFlowService>();
            var context = new RecoveryContext
            {
                ClientId = clientID
            };

            flow.Context.Returns(context);
            _clientAccountClient.GetByIdAsync("").ReturnsForAnyArgs(new ClientModel());

            _confirmationCodesClient.VerifyEmailCodeAsync(null).ReturnsForAnyArgs(new VerificationResult { IsValid = isValid });

            await _emailValidator.Confirm(flow, "SomeCode");

            if (isValid)
            {
                await flow.Received().EmailVerificationCompleteAsync();
                await flow.DidNotReceive().EmailVerificationFailedAsync();
            }
            else
            {
                await flow.DidNotReceive().EmailVerificationCompleteAsync();
                await flow.Received().EmailVerificationFailedAsync();
            }
        }

        [TestCase("Correct code", true)]
        [TestCase("Invalid code", false)]
        public async Task Should_CorrectlyValidate_Device(string code, bool isValid)
        {
            const string challenge = "Correct code";
            var clientID = Guid.NewGuid().ToString();
            var key = Key.Parse(PrivateKey);

            var signature = key.SignMessage(code);


            var flow = Substitute.For<IRecoveryFlowService>();
            var context = new RecoveryContext
            {
                ClientId = clientID,
                SignChallengeMessage = challenge
            };
            flow.Context.Returns(context);

            var credentials = Substitute.For<IWalletCredentials>();
            credentials.Address.Returns(PublicAddress);
            _credentialsRepository.GetAsync(clientID).Returns(Task.FromResult(credentials));

            await _deviceValidator.Confirm(flow, signature);

            if (isValid)
            {
                await flow.Received().DeviceVerifiedCompleteAsync();
                await flow.DidNotReceive().DeviceVerificationFailAsync();
            }
            else
            {
                await flow.DidNotReceive().DeviceVerifiedCompleteAsync();
                await flow.Received().DeviceVerificationFailAsync();
            }
        }


        [TestCase("Correct code", true)]
        [TestCase("Invalid code", false)]
        public async Task Should_CorrectlyValidate_Pin(string code, bool isValid)
        {
            const string pinCode = "1234";
            var clientID = Guid.NewGuid().ToString();

            var flow = Substitute.For<IRecoveryFlowService>();
            var context = new RecoveryContext
            {
                ClientId = clientID,
                SignChallengeMessage = pinCode
            };
            flow.Context.Returns(context);
            _clientAccountClient.IsPinValidAsync(clientID, pinCode).Returns(Task.FromResult(isValid));


            await _pinValidator.Confirm(flow, pinCode);


            await _clientAccountClient.Received().IsPinValidAsync(clientID, pinCode);
            if (isValid)
            {
                await flow.Received().PinCodeVerificationCompleteAsync();
                await flow.DidNotReceive().PinCodeVerificationFailAsync();
            }
            else
            {
                await flow.DidNotReceive().PinCodeVerificationCompleteAsync();
                await flow.Received().PinCodeVerificationFailAsync();
            }
        }

        [Test]
        public void Should_Throw_IfAddress_IsEmpty()
        {
            const string challenge = "Correct code";
            var clientID = Guid.NewGuid().ToString();

            var flow = Substitute.For<IRecoveryFlowService>();
            var context = new RecoveryContext
            {
                ClientId = clientID,
                SignChallengeMessage = challenge
            };
            flow.Context.Returns(context);

            var credentials = Substitute.For<IWalletCredentials>();
            credentials.Address.Returns("");

            _credentialsRepository.GetAsync(clientID).Returns(Task.FromResult(credentials));

            Assert.ThrowsAsync<InvalidOperationException>(() => _deviceValidator.Confirm(flow, "signed message"));

        }
    }
}
