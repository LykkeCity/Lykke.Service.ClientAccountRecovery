using System.Collections.Generic;
using Autofac;
using AutoMapper;
using Lykke.Service.ClientAccountRecovery.AzureRepositories;

namespace Lykke.Service.ClientAccountRecovery.Modules
{
    internal class AutoMapperModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MappingProfile>()
                .As<Profile>();
            builder.Register(c => new MapperConfiguration(cfg =>
            {
                foreach (var profile in c.Resolve<IEnumerable<Profile>>())
                {
                    cfg.AddProfile(profile);
                }
            })).AsSelf().SingleInstance();

            builder.Register(c => c.Resolve<MapperConfiguration>()
                    .CreateMapper(c.Resolve))
                .As<IMapper>()
                .SingleInstance();
        }
    }
}
