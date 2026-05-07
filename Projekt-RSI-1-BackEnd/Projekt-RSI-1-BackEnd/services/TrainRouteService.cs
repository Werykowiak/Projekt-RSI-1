using CoreWCF;
using Microsoft.EntityFrameworkCore;
using Projekt_RSI_1_BackEnd.Interfaces;
using Projekt_RSI_1_BackEnd.Models;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

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

        public async Task<List<TrainRoute>> SearchTrainRoutes(string departureCity, string arrivalCity, DateTime? departureDay, string targetCurrency = "PLN")
        {
            try
            {
                var query = _context.TrainRoutes.AsQueryable();

                if (!string.IsNullOrEmpty(departureCity))
                    query = query.Where(r => r.departureCity.ToLower().Contains(departureCity.ToLower()));

                if (!string.IsNullOrEmpty(arrivalCity))
                    query = query.Where(r => r.arrivalCity.ToLower().Contains(arrivalCity.ToLower()));

                if (departureDay.HasValue)
                    query = query.Where(r => r.departureTime.Date == departureDay.Value.Date);

                var routes = await query.ToListAsync();

                if (!string.IsNullOrEmpty(targetCurrency) && targetCurrency.ToUpper() != "PLN")
                {
                    string exchangerUrl = "https://exchanger:8080/CurrencyService";

                    // 1. Konfiguracja Bindingu (HTTPS)
                    var binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.Transport);

                    // 2. Definicja Endpointu
                    var endpoint = new System.ServiceModel.EndpointAddress(exchangerUrl);

                    // 3. NOWOCZESNE ROZWIĄZANIE: Tworzymy klienta
                    var currencyClient = new ServiceReference1.CurrencyServiceClient(binding, endpoint);

                    // 4. Konfiguracja ignorowania certyfikatów dla HttpClient (wersja dla WCF/CoreWCF)
                    // W nowych wersjach .NET musimy dostać się do parametrów fabryki
                    currencyClient.Endpoint.EndpointBehaviors.Add(new CustomCertificateBehavior());



                    double multiplier = await currencyClient.GetMultiplierAsync(targetCurrency);

                    foreach (var route in routes)
                    {
                        route.price = route.price * (decimal)multiplier;
                    }
                }

                return routes;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
    public class CustomCertificateBehavior : IEndpointBehavior
    {
        // Ta metoda dodaje naszą "magiczną funkcję" ignorowania błędów SSL do HttpClienta
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            bindingParameters.Add(new Func<System.Net.Http.HttpClientHandler, System.Net.Http.HttpClientHandler>(handler =>
            {
                // Pozwalamy na każdy certyfikat (ważne w Dockerze)
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                return handler;
            }));
        }

        // Ta metoda musi tu być, ale zostawiamy ją pustą (standard WCF)
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) { }

        // POPRAWIONA METODA: Używamy EndpointDispatcher zamiast filtra
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

        // Ta metoda również zostaje pusta
        public void Validate(ServiceEndpoint endpoint) { }
    }
}
