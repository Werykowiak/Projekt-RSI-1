
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt_RSI_1_BackEnd.Models
{
    public class Reservation
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("TrainRoute")]
        [Required]
        public int trainRouteId { get; set; }
        [Required]
        public string passengerFirstName { get; set; }
        [Required]
        public string passengerLastName { get; set; }
        [Required]
        public string passengerEmail { get; set; }
        [Required]
        public DateTime reservationDate { get; set; }
        [Required]
        public int numberOfSeats { get; set; }
    }
}
