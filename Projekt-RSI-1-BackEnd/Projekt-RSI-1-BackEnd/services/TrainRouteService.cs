using CoreWCF;
using Projekt_RSI_1_BackEnd.Interfaces;
using Projekt_RSI_1_BackEnd.Models;

namespace Projekt_RSI_1_BackEnd.services
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class TrainRouteService : ITrainRouteService
    {
        private readonly AppDbContext _context;

        public TrainRouteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TrainRoute> AddTrainRoute(TrainRoute trainRoute)
        {
            try 
            {
                await _context.TrainRoutes.AddAsync(trainRoute);
                await _context.SaveChangesAsync();
                return trainRoute;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
