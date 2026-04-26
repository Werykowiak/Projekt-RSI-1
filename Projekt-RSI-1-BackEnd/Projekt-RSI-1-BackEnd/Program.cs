using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.EntityFrameworkCore;
using Projekt_RSI_1_BackEnd.Handlers;
using Projekt_RSI_1_BackEnd.Interfaces;
using Projekt_RSI_1_BackEnd.services;

namespace Projekt_RSI_1_BackEnd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));

            builder.Services.AddServiceModelServices();
            builder.Services.AddServiceModelMetadata();
            builder.Services.AddTransient<TrainRouteService>();
            builder.Services.AddSingleton<ServiceDebugBehavior>(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(8181, listenOptions =>
                {
                    listenOptions.UseHttps(); // Używa domyślnego certyfikatu dev .NET
                });
            });

            var app = builder.Build();

            app.UseServiceModel(serviceBuilder =>
            {
                var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                binding.MessageEncoding = WSMessageEncoding.Mtom;
                binding.MaxReceivedMessageSize = 10 * 1024 * 1024;

                serviceBuilder.AddService<TrainRouteService>();
                serviceBuilder.ConfigureServiceHostBase<TrainRouteService>(host =>
                {
                    string apiKeyFromConfig = builder.Configuration["Keys:ApiKey"];
                    host.Description.Behaviors.Add(new ApiKeyBehavior(apiKeyFromConfig));
                });
                serviceBuilder.AddServiceEndpoint<TrainRouteService, ITrainRouteService>(binding, "/TrainRouteService");

                var metadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
                metadataBehavior.HttpsGetEnabled = true;

                var debugBehavior = app.Services.GetRequiredService<ServiceDebugBehavior>();
                debugBehavior.IncludeExceptionDetailInFaults = true;

            });
            app.Run();
        }
    }
}
