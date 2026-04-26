using CoreWCF;
using Microsoft.EntityFrameworkCore;
using Projekt_RSI_1_BackEnd.Interfaces;
using Projekt_RSI_1_BackEnd.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projekt_RSI_1_BackEnd.services
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;

        public ReservationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Reservation> BuyTicket(int trainRouteId, string firstName, string lastName, string email, int numberOfSeats)
        {
            var route = await _context.TrainRoutes.FirstOrDefaultAsync(r => r.id == trainRouteId);
            if (route == null)
            {
                throw new Exception("Trasa o podanym ID nie istnieje");
            }

            if (route.availableSeats < numberOfSeats)
            {
                throw new Exception("Brak wystarczajacej liczby miejsc");
            }

            var reservation = new Reservation
            {
                trainRouteId = trainRouteId,
                passengerFirstName = firstName,
                passengerLastName = lastName,
                passengerEmail = email,
                reservationDate = DateTime.Now,
                numberOfSeats = numberOfSeats
            };

            route.availableSeats -= numberOfSeats;

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return reservation;
        }

        public async Task<Reservation?> GetReservation(int reservationId)
        {
            return await _context.Reservations.FirstOrDefaultAsync(r => r.id == reservationId);
        }

        public async Task<byte[]> GenerateReservationPdf(int reservationId)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.id == reservationId);
            if (reservation == null)
            {
                throw new Exception("Rezerwacja o podanym ID nie istnieje.");
            }

            var route = await _context.TrainRoutes.FirstOrDefaultAsync(r => r.id == reservation.trainRouteId);

            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text("Bilet na pociąg")
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Spacing(10);

                        x.Item().Text($"Imie i nazwisko: {reservation.passengerFirstName} {reservation.passengerLastName}");
                        x.Item().Text($"Email: {reservation.passengerEmail}");
                        x.Item().Text($"ID Rezerwacji: {reservation.id}");
                        x.Item().Text($"Data Rezerwacji: {reservation.reservationDate}");
                        x.Item().Text($"Liczba miejsc: {reservation.numberOfSeats}");

                        if (route != null)
                        {
                            x.Item().PaddingTop(10).Text($"Trasa: {route.departureCity} -> {route.arrivalCity}").Bold();
                            x.Item().Text($"Data odjazdu: {route.departureTime}");
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("DAWIDZIOR & WERYK TRAINS 😎");
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}