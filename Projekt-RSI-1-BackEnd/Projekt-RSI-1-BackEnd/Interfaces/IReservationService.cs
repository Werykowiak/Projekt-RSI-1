using Projekt_RSI_1_BackEnd.Models;
using CoreWCF;

namespace Projekt_RSI_1_BackEnd.Interfaces
{
    [ServiceContract]
    public interface IReservationService
    {
        [OperationContract]
        Task<Reservation> BuyTicket(int trainRouteId, string firstName, string lastName, string email, int numberOfSeats);

        [OperationContract]
        Task<Reservation?> GetReservation(int reservationId);

        [OperationContract]
        Task<byte[]> GenerateReservationPdf(int reservationId);
    }
}