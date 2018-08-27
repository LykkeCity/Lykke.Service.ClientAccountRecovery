using System.Collections.Generic;
using Autofac;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Settings;
using Lykke.SettingsReader;
using RabbitMQ.Client;

namespace Lykke.Service.Session.Modules
{
    internal class CqrsModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            MessagePackSerializerFactory.Defaults.FormatterResolver = MessagePack.Resolvers.ContractlessStandardResolver.Instance;

            builder.RegisterType<AutofacDependencyResolver>().As<IDependencyResolver>().SingleInstance();

            builder.Register(c => new ConnectionFactory { Uri = c.Resolve<IReloadingManager<AppSettings>>().Nested(n => n.ClientAccountRecoveryService.RabbitMq.ConnectionString).CurrentValue });

            builder.Register(c =>
            {
                var rabbitMqSettings = c.Resolve<ConnectionFactory>();
                return new MessagingEngine(c.Resolve<ILogFactory>(),
                    new TransportResolver(new Dictionary<string, TransportInfo>
                    {
                        {"RabbitMq", new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName, rabbitMqSettings.Password, "None", "RabbitMq")}
                    }),
                    new RabbitMqTransportFactory(c.Resolve<ILogFactory>()));
            });

            builder.Register(ctx => new CqrsEngine(ctx.Resolve<ILogFactory>(),
                    ctx.Resolve<IDependencyResolver>(),
                    ctx.Resolve<MessagingEngine>(),
                    new DefaultEndpointProvider(),
                    true,
                    Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                        "RabbitMq",
                        SerializationFormat.MessagePack,
                        environment: "lykke",
                        exclusiveQueuePostfix: "k8s")),

                    Register.BoundedContext(Consts.BoundedContext)
                        .PublishingEvents(typeof(SelfiePostedEvent)).With("events")
                ))
                .As<ICqrsEngine>()
                .SingleInstance();
        }
    }
}
