using AutoMapper;
using Lykke.Service.ClientAccountRecovery.AzureRepositories;
using NUnit.Framework;

namespace Lykke.Service.ClientAccountRecovery.Tests
{
    [TestFixture]
    public class MappingTest
    {
        [Test]
        public void MappingProfile_Configured_Correctly()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile(new MappingProfile()); });
            config.AssertConfigurationIsValid();
        }
    }
}
