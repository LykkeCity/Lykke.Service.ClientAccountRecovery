using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ClientAccountRecovery.Settings;
using Lykke.Service.ClientAccountRecovery.Modules;
using Lykke.SettingsReader;
using Lykke.MonitoringServiceApiCaller;
using Lykke.Service.ClientAccountRecovery.Middleware;
using Lykke.Service.ClientAccountRecovery.Models;
using Lykke.Service.Session.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;

namespace Lykke.Service.ClientAccountRecovery
{
    [UsedImplicitly]
    internal class Startup
    {
        private string _monitoringServiceUrl;

        private IContainer ApplicationContainer { get; set; }
        private IConfigurationRoot Configuration { get; }
        private ILog Log { get; set; }
        private IHealthNotifier HealthNotifier { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver =
                            new DefaultContractResolver();
                    });

                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", "ClientAccountRecovery API");
                    options.OperationFilter<AddRequiredHeaderParameter>();
                    options.AddSecurityDefinition("CustomScheme", new ApiKeyScheme { In = "header", Description = "Please insert API key into field", Name = ApiKeyAuthAttribute.HeaderName, Type = "apiKey" });
                });
                services.Configure<MvcJsonOptions>(c =>
                {
                    // Serialize all properties to camelCase by default
                    c.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });


                var builder = new ContainerBuilder();
                var appSettings = Configuration.LoadSettings<AppSettings>(options =>
                {
                    options.SetConnString(x => x.SlackNotifications.AzureQueue.ConnectionString);
                    options.SetQueueName(x => x.SlackNotifications.AzureQueue.QueueName);
                    options.SenderName = "ClientAccountRecovery";
                });


                builder.RegisterInstance(appSettings).As<IReloadingManager<AppSettings>>();
                services.AddLykkeLogging(appSettings.Nested(x => x.ClientAccountRecoveryService.Db.LogsConnString),
                    "ClientAccountRecoveryLog",
                    appSettings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                    appSettings.CurrentValue.SlackNotifications.AzureQueue.QueueName
                );


                if (appSettings.CurrentValue.MonitoringServiceClient != null)
                    _monitoringServiceUrl = appSettings.CurrentValue.MonitoringServiceClient.MonitoringServiceUrl;

                ApiKeyAuthAttribute.ApiKey = appSettings.CurrentValue.ClientAccountRecoveryService.ApiKey;

                builder.RegisterInstance(appSettings).As<IReloadingManagerWithConfiguration<AppSettings>>();
                builder.RegisterModule(new ServiceModule(appSettings));
                builder.RegisterModule<CqrsModule>();
                builder.RegisterModule<AutoMapperModule>();
                builder.Populate(services);
                ApplicationContainer = builder.Build();
                Log = ApplicationContainer.Resolve<ILogFactory>().CreateLog(this);
                HealthNotifier = ApplicationContainer.Resolve<IHealthNotifier>();
                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseLykkeForwardedHeaders();
                app.UseLykkeMiddleware(ex => new OperationStatus { Error = true, Message = "Technical problem" });

                app.UseMvc();
                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
                });
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                });
                app.UseStaticFiles();


                appLifetime.ApplicationStarted.Register(() => StartApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopping.Register(() => StopApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopped.Register(CleanUp);
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                // NOTE: Service not yet recieve and process requests here

                await ApplicationContainer.Resolve<IStartupManager>().StartAsync();

                HealthNotifier.Notify($"Env: {Program.EnvInfo}", "Started");
                if (_monitoringServiceUrl != null)
                {
                    await Configuration.RegisterInMonitoringServiceAsync(_monitoringServiceUrl, HealthNotifier);
                }
            }
            catch (Exception ex)
            {
                Log.Critical(ex);
                throw;
            }
        }

        private async Task StopApplication()
        {
            try
            {
                // NOTE: Service still can recieve and process requests here, so take care about it if you add logic here.

                await ApplicationContainer.Resolve<IShutdownManager>().StopAsync();
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        private void CleanUp()
        {
            try
            {
                // NOTE: Service can't receive and process requests here, so you can destroy all resources

                HealthNotifier?.Notify($"Env: {Program.EnvInfo}", "Terminating");

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }
    }
}
