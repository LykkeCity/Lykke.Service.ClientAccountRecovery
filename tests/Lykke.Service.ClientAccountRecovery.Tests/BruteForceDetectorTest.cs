using System;
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
    public class BruteForceDetectorTest
    {
        private BruteForceDetector _detector;
        private IStateRepository _stateRepository;
        private IRecoveryFlowServiceFactory _serviceFactory;
        private const string clientID = "1";

        [SetUp]
        public void Setup()
        {
            _stateRepository = Substitute.For<IStateRepository>();
            _serviceFactory = Substitute.For<IRecoveryFlowServiceFactory>();
            var rc = new RecoveryConditions();
            _detector = new BruteForceDetector(_stateRepository, _serviceFactory, rc);
        }

        [Test]
        public async Task IsNewRecoveryAllowedAsync_WhenThreeForbiddenAttemptsMade_ShouldBlockNext([Values] State fourthState)
        {
            AddRecovery(State.PasswordChangeForbidden, State.PasswordChangeForbidden, State.PasswordChangeForbidden, fourthState); // By default 3 unsuccessful attempts allowed
            var result = await _detector.IsNewRecoveryAllowedAsync(clientID);
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public async Task IsNewRecoveryAllowedAsync_WhenTwoForbiddenAttemptsDone_ShouldAllowNext()
        {
            AddRecovery(State.PasswordChangeForbidden, State.PasswordChangeForbidden); // By default 3 unsuccessful attempts allowed
            var result = await _detector.IsNewRecoveryAllowedAsync(clientID);
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public async Task GetRecoveriesToSeal_WhenOngoingRecoveriesArePresent_ShouldReturnThem()
        {
            var flow1 = Substitute.For<IRecoveryFlowService>();
            flow1.Context.Returns(new RecoveryContext());
            var flow2 = Substitute.For<IRecoveryFlowService>();
            flow2.Context.Returns(new RecoveryContext());
            var flow3 = Substitute.For<IRecoveryFlowService>();
            flow3.Context.Returns(new RecoveryContext());

            var recovery = AddRecovery(State.AwaitDeviceVerification, State.AwaitPinCode, State.PasswordChangeForbidden);

            _serviceFactory.FindExisted(recovery.Log[0].RecoveryId).Returns(Task.FromResult(flow1));
            _serviceFactory.FindExisted(recovery.Log[1].RecoveryId).Returns(Task.FromResult(flow2));
            _serviceFactory.FindExisted(recovery.Log[2].RecoveryId).Returns(Task.FromResult(flow3));

            var actual = await _detector.GetRecoveriesToSeal(clientID);

            Assert.That(actual.Count, Is.EqualTo(2));

            Assert.That(actual.Select(a => a.RecoveryId), Has.One.EqualTo(recovery.Log[0].RecoveryId));
            Assert.That(actual.Select(a => a.RecoveryId), Has.One.EqualTo(recovery.Log[1].RecoveryId));

        }


        [Test]
        public async Task DoNotSearchRecoveries_WhenItIsFirstRecovery()
        {

            await _detector.GetRecoveriesToSeal(clientID);
            await _serviceFactory.DidNotReceiveWithAnyArgs().FindExisted("");
        }

        [Test]
        public async Task ShouldNotBlockNewcomers()
        {
            var result = await _detector.IsNewRecoveryAllowedAsync(clientID);
            Assert.That(result, Is.EqualTo(true));
        }

        private RecoveriesSummaryForClient AddRecovery(params State[] states)
        {
            var summary = new RecoveriesSummaryForClient(clientID);

            for (var i = 0; i < states.Length; i++)
            {
                var state = states[i];
                summary.AddItem(new RecoveryUnit(new[]
                {
                    new RecoveryContext
                    {
                        RecoveryId = Guid.NewGuid().ToString(),
                        ClientId = clientID,
                        State = state,
                        SeqNo = i,
                        Time = DateTime.UtcNow.AddDays(-i)
                    }
                }));
            }

            _stateRepository.FindRecoverySummary(clientID).Returns(Task.FromResult(summary));
            return summary;
        }
    }
}
