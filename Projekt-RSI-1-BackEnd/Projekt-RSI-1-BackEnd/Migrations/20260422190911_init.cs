using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projekt_RSI_1_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrainRoutes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    departureCity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    arrivalCity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    departureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    arrivalTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    availableSeats = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainRoutes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    trainRouteId = table.Column<int>(type: "int", nullable: false),
                    passengerFirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    passengerLastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    passengerEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    reservationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    numberOfSeats = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.id);
                    table.ForeignKey(
                        name: "FK_Reservations_TrainRoutes_trainRouteId",
                        column: x => x.trainRouteId,
                        principalTable: "TrainRoutes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_trainRouteId",
                table: "Reservations",
                column: "trainRouteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "TrainRoutes");
        }
    }
}
