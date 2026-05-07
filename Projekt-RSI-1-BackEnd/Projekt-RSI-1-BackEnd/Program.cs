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
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DBConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null)
                ));
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowNuxt", policy =>
                {
                    policy.WithOrigins(
                        "http://localhost:3000",
                        "https://localhost:3000",
                        "https://localhost:8180",
                        "https://127.0.0.1:8180",
                        "http://127.0.0.1:3000",
                        "https://127.0.0.1:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            builder.Services.AddServiceModelServices();
            builder.Services.AddServiceModelMetadata();
            builder.Services.AddTransient<TrainRouteService>();
            builder.Services.AddTransient<ReservationService>();
            builder.Services.AddSingleton<ServiceDebugBehavior>(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(8181, listenOptions =>
                {
                    listenOptions.UseHttps(); // Używa domyślnego certyfikatu dev .NET lub skonfigurowanego PFX
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


                serviceBuilder.AddService<TrainRouteService>();
                serviceBuilder.ConfigureServiceHostBase<TrainRouteService>(host =>
                {
                    string apiKeyFromConfig = builder.Configuration["Keys:ApiKey"];
                    host.Description.Behaviors.Add(new ApiKeyBehavior(apiKeyFromConfig));
                });
                serviceBuilder.AddServiceEndpoint<TrainRouteService, ITrainRouteService>(binding, "/TrainRouteService");

                serviceBuilder.AddService<ReservationService>();
                serviceBuilder.AddServiceEndpoint<ReservationService, IReservationService>(binding, "/ReservationService");

                var metadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
                metadataBehavior.HttpsGetEnabled = true;

                var debugBehavior = app.Services.GetRequiredService<ServiceDebugBehavior>();
                debugBehavior.IncludeExceptionDetailInFaults = true;

            });
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // Ta metoda automatycznie utworzy bazę i wszystkie tabele na podstawie Twoich migracji
                dbContext.Database.Migrate();
            }
            app.Run();
        }
    }
}
