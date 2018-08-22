using System.Threading.Tasks;
using Autofac;
using Lykke.Service.ClientAccountRecovery.Client;
using Lykke.Service.ClientAccountRecovery.Client.AutoRestClient.Models;
using NUnit.Framework;

namespace Lykke.Service.ClientAccountRecovery.Tests
{
    [TestFixture, Ignore("Only for manual running")]
    public class ClientTest
    {
        private IContainer _container;

        public ClientTest()
        {
            var builder = new ContainerBuilder();
            builder.RegisterClientAccountRecoveryClient("http://localhost:5000", "1");
            _container = builder.Build();
        }

        [Test]
        public async Task RegistrationTest()
        {

            var client = _container.Resolve<IAccountRecoveryService>();
            var callResult = await client.StartNewRecoveryAsync(new NewRecoveryRequest("sdfddfsdfdfsf", "myIp", "MyUserAgent"));
            Assert.NotNull(callResult);
        }

        [Test]
        public void ShouldThrowUnauthorizedException()
        {
            var builder = new ContainerBuilder();
            builder.RegisterClientAccountRecoveryClient("http://localhost:5000", "");
            var container = builder.Build();

            var client = container.Resolve<IAccountRecoveryService>();
            Assert.ThrowsAsync<UnauthorizedException>(() => client.ApproveChallengeAsync(new ApproveChallengeRequest
            {
                AgentId = "dfdfd",
                Challenge = Challenge.Device,
                CheckResult = CheckResult.Approved,
                RecoveryId = "sdffeererere"
            }));
        }

        [Test]
        public void ShouldThrowNotFoundException()
        {
            var client = _container.Resolve<IAccountRecoveryService>();
            Assert.ThrowsAsync<NotFoundException>(() => client.ApproveChallengeAsync(new ApproveChallengeRequest
            {
                AgentId = "dfdfd",
                Challenge = Challenge.Device,
                CheckResult = CheckResult.Approved,
                RecoveryId = "sdffeererere"
            }));
        }
    }
}
