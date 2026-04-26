using CoreWCF;
using Projekt_RSI_1_BackEnd.Models;

namespace Projekt_RSI_1_BackEnd.Interfaces
{
    [ServiceContract]
    public interface ITrainRouteService
    {
        [OperationContract]
        Task<TrainRoute> AddTrainRoute(TrainRoute trainRoute);
        [OperationContract]
        Task<bool> DeleteTrainRoute(int id);
        [OperationContract]
        Task<TrainRoute> GetTrainRoute(int id);
        [OperationContract]
        Task<List<TrainRoute>> GetAllTrainRoutes();
        [OperationContract]
        Task<TrainRoute> EditTrainRoute(TrainRoute trainRoute);
        [OperationContract]
        Task<List<TrainRoute>> SearchTrainRoutes(string departureCity, string arrivalCity, DateTime? departureDate);
    }
}
