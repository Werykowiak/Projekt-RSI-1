using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Projekt_RSI_1_BackEnd.Models
{
    public class TrainRoute
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string departureCity { get; set; }
        [Required]
        public string arrivalCity { get; set; }
        [Required]
        public DateTime departureTime { get; set; }
        public DateTime arrivalTime { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        [Precision(18, 2)]
        public decimal price { get; set; }
        public int availableSeats { get; set; }
    }
}
