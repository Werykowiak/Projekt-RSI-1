using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.EntityFrameworkCore;

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

            var app = builder.Build();

            app.UseServiceModel(serviceBuilder =>
            {
                var metadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
                metadataBehavior.HttpGetEnabled = true;


            });
            app.Run();
        }
    }
}
