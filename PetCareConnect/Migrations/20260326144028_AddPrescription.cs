using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetCareConnect.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    PrescriptionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedicationName = table.Column<string>(type: "text", nullable: false),
                    Dosage = table.Column<string>(type: "text", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RefillStatus = table.Column<string>(type: "text", nullable: false),
                    PetID = table.Column<int>(type: "integer", nullable: false),
                    ApprovedByUserID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.PrescriptionID);
                    table.ForeignKey(
                        name: "FK_Prescriptions_AspNetUsers_ApprovedByUserID",
                        column: x => x.ApprovedByUserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Pets_PetID",
                        column: x => x.PetID,
                        principalTable: "Pets",
                        principalColumn: "PetID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_ApprovedByUserID",
                table: "Prescriptions",
                column: "ApprovedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_PetID",
                table: "Prescriptions",
                column: "PetID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prescriptions");
        }
    }
}
