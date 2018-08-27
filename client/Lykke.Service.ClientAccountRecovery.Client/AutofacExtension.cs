using System;
using System.Net.Http;
using Autofac;
using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    /// <summary>
    /// A helper class for the client registration
    /// </summary>
    [PublicAPI]
    public static class AutofacExtension
    {
        /// <summary>
        /// Registers a client implementation in the container
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serviceUrl"></param>
        /// <param name="apiKey"></param>
        public static void RegisterClientAccountRecoveryClient([NotNull] this ContainerBuilder builder, [NotNull] string serviceUrl, [CanBeNull] string apiKey)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            var cred = new ClientCredentials(apiKey);
            builder.Register(c => new AccountRecoveryService(new Uri(serviceUrl), new HttpClient(), cred))
                .As<IAccountRecoveryService>()
                .SingleInstance();
        }

        /// <summary>
        /// Registers a client implementation in the container
        /// </summary>
        public static void RegisterClientAccountRecoveryClient(this ContainerBuilder builder, [NotNull] ClientAccountRecoveryServiceClientSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            builder.RegisterClientAccountRecoveryClient(settings.ServiceUrl, settings.ApiKey);
        }
    }
}
