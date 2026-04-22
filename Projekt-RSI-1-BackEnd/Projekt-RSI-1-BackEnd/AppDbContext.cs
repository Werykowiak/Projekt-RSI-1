using Microsoft.EntityFrameworkCore;
using Projekt_RSI_1_BackEnd.Models;

namespace Projekt_RSI_1_BackEnd
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<TrainRoute> TrainRoutes { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reservation>()
                .HasOne<TrainRoute>()
                .WithMany()
                .HasForeignKey(r => r.trainRouteId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
