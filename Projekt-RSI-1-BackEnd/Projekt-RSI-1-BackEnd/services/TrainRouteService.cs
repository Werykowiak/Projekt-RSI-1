using CoreWCF;
using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> DeleteTrainRoute(int id)
        {
            try
            {
                var trainRoute = await _context.TrainRoutes.FindAsync(id);
                if (trainRoute == null)
                    return false;
                _context.TrainRoutes.Remove(trainRoute);
                await _context.SaveChangesAsync();
                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TrainRoute> EditTrainRoute(TrainRoute trainRoute)
        {
            try
            {
                var existingRoute = await _context.TrainRoutes.FindAsync(trainRoute.id);
                if (existingRoute == null)
                    return null;
                existingRoute.departureCity = trainRoute.departureCity;
                existingRoute.arrivalCity = trainRoute.arrivalCity;
                existingRoute.departureTime = trainRoute.departureTime;
                existingRoute.arrivalTime = trainRoute.arrivalTime;
                existingRoute.price = trainRoute.price;
                existingRoute.availableSeats = trainRoute.availableSeats;
                await _context.SaveChangesAsync();
                return existingRoute;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TrainRoute>> GetAllTrainRoutes()
        {
            try
            {
                return await _context.TrainRoutes.ToListAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TrainRoute> GetTrainRoute(int id)
        {
            try
            {
                return await _context.TrainRoutes.FindAsync(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TrainRoute>> SearchTrainRoutes(string departureCity, string arrivalCity, DateTime? departureDay)
        {
            try
            {
                var query = _context.TrainRoutes.AsQueryable();
                if (!string.IsNullOrEmpty(departureCity))
                    query = query.Where(r => r.departureCity.ToLower().Contains(departureCity.ToLower()));
                if (!string.IsNullOrEmpty(arrivalCity))
                    query = query.Where(r => r.arrivalCity.ToLower().Contains(arrivalCity.ToLower()));
                if (departureDay.HasValue)
                    query = query.Where(r => r.departureTime.Date.DayOfYear == departureDay.Value.Date.DayOfYear && r.departureTime.Date.Year == departureDay.Value.Date.Year);
                return await query.ToListAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
