using CoreWCF;
using Projekt_RSI_1_BackEnd.Models;

namespace Projekt_RSI_1_BackEnd.Interfaces
{
    [ServiceContract]
    public interface ITrainRouteService
    {
        [OperationContract]
        Task<TrainRoute> AddTrainRoute(TrainRoute trainRoute);
    }
}
