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
    public class ChallengeManagerTest
    {
        private ChallengeManager _challengeManager;
        private IChallengeValidatorFactory _challengeValidatorFactory;
        private ISelfieNotificationSender _selfieNotificationSender;
        private IRecoveryFlowService _flowService;
        private IChallengesValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _challengeValidatorFactory = Substitute.For<IChallengeValidatorFactory>();
            _selfieNotificationSender = Substitute.For<ISelfieNotificationSender>();
            _flowService = Substitute.For<IRecoveryFlowService>();
            _challengeManager = new ChallengeManager(_challengeValidatorFactory, _selfieNotificationSender);
            _validator = Substitute.For<IChallengesValidator>();
            _challengeValidatorFactory.GetValidator(Arg.Any<Challenge>()).Returns(_validator);
        }

        [Test]
        public async Task Should_Validate_Sms()
        {
            await _challengeManager.ExecuteAction(Challenge.Sms, Action.Complete, "42", _flowService);

            _challengeValidatorFactory.Received().GetValidator(Challenge.Sms);
            await _validator.Received().Confirm(_flowService, "42");
        } 
        
        [Test]
        public async Task Should_Validate_Phrases()
        {
            await _challengeManager.ExecuteAction(Challenge.Words, Action.Complete, "42", _flowService);

            _challengeValidatorFactory.Received().GetValidator(Challenge.Words);
            await _validator.Received().Confirm(_flowService, "42");
        }   
        
        [Test]
        public async Task Should_Validate_Device()
        {
            await _challengeManager.ExecuteAction(Challenge.Device, Action.Complete, "42", _flowService);

            _challengeValidatorFactory.Received().GetValidator(Challenge.Device);
            await _validator.Received().Confirm(_flowService, "42");
        }


        [Test]
        public async Task Should_Validate_Pin()
        {
            await _challengeManager.ExecuteAction(Challenge.Pin, Action.Complete, "42", _flowService);

            _challengeValidatorFactory.Received().GetValidator(Challenge.Pin);
            await _validator.Received().Confirm(_flowService, "42");
        }

        [Test]
        public async Task Should_Validate_Email()
        {
            await _challengeManager.ExecuteAction(Challenge.Email, Action.Complete, "42", _flowService);

            _challengeValidatorFactory.Received().GetValidator(Challenge.Email);
            await _validator.Received().Confirm(_flowService, "42");
        }

        [Test]
        public async Task ShouldSendSelfieCode()
        {
            await _challengeManager.ExecuteAction(Challenge.Selfie, Action.Complete, "42", _flowService);

            await _selfieNotificationSender.Received().Send(_flowService, "42");
        }
    }
}
