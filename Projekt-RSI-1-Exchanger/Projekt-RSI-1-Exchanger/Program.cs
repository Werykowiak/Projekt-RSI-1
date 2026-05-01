using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Projekt_RSI_1_Exchanger.Interfaces;
using Projekt_RSI_1_Exchanger.Services;

namespace Projekt_RSI_1_Exchanger
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowNuxt", policy =>
                {
                    policy.WithOrigins("https://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            builder.Services.AddServiceModelServices();
            builder.Services.AddServiceModelMetadata();
            builder.Services.AddTransient<ICurrencyService, CurrencyService>();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(8180, listenOptions =>
                {
                    listenOptions.UseHttps(); // U¿ywa domyœlnego certyfikatu dev .NET
                });
            });

            var app = builder.Build();

            app.UseRouting();
            app.UseCors("AllowNuxt");

            app.UseServiceModel(serviceBuilder =>
            {
                var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                binding.MessageEncoding = WSMessageEncoding.Mtom;
                binding.MaxReceivedMessageSize = 10 * 1024 * 1024;

                serviceBuilder.AddService<CurrencyService>();
                serviceBuilder.AddServiceEndpoint<CurrencyService, ICurrencyService>(binding, "/CurrencyService");

                var metadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
                metadataBehavior.HttpsGetEnabled = true;


            });

            app.Run();
        }
    }
}
