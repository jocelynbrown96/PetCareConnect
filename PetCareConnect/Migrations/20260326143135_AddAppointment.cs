using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetCareConnect.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    AppointmentID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppointmentDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServiceType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: true),
                    PetID = table.Column<int>(type: "integer", nullable: false),
                    UserID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.AppointmentID);
                    table.ForeignKey(
                        name: "FK_Appointments_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Pets_PetID",
                        column: x => x.PetID,
                        principalTable: "Pets",
                        principalColumn: "PetID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PetID",
                table: "Appointments",
                column: "PetID");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_UserID",
                table: "Appointments",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");
        }
    }
}
