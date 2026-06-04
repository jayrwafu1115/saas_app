using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductionHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "security_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Subject = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DetailsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_patients_TenantId_CreatedAtUtc",
                table: "patients",
                columns: new[] { "TenantId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_patients_TenantId_LastName_FirstName",
                table: "patients",
                columns: new[] { "TenantId", "LastName", "FirstName" });

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_TenantId_ClinicianUserId_EncounterDateUtc",
                table: "Encounters",
                columns: new[] { "TenantId", "ClinicianUserId", "EncounterDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_TenantId_Status_EncounterDateUtc",
                table: "Encounters",
                columns: new[] { "TenantId", "Status", "EncounterDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BillingPayments_TenantId_Status_CreatedAtUtc",
                table: "BillingPayments",
                columns: new[] { "TenantId", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_TenantId_Status_StartsAtUtc",
                table: "appointments",
                columns: new[] { "TenantId", "Status", "StartsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerations_TenantId_Status_CreatedAtUtc",
                table: "AIGenerations",
                columns: new[] { "TenantId", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_security_audit_logs_EventType_CreatedAtUtc",
                table: "security_audit_logs",
                columns: new[] { "EventType", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_security_audit_logs_TenantId_CreatedAtUtc",
                table: "security_audit_logs",
                columns: new[] { "TenantId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_security_audit_logs_UserId_CreatedAtUtc",
                table: "security_audit_logs",
                columns: new[] { "UserId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "security_audit_logs");

            migrationBuilder.DropIndex(
                name: "IX_patients_TenantId_CreatedAtUtc",
                table: "patients");

            migrationBuilder.DropIndex(
                name: "IX_patients_TenantId_LastName_FirstName",
                table: "patients");

            migrationBuilder.DropIndex(
                name: "IX_Encounters_TenantId_ClinicianUserId_EncounterDateUtc",
                table: "Encounters");

            migrationBuilder.DropIndex(
                name: "IX_Encounters_TenantId_Status_EncounterDateUtc",
                table: "Encounters");

            migrationBuilder.DropIndex(
                name: "IX_BillingPayments_TenantId_Status_CreatedAtUtc",
                table: "BillingPayments");

            migrationBuilder.DropIndex(
                name: "IX_appointments_TenantId_Status_StartsAtUtc",
                table: "appointments");

            migrationBuilder.DropIndex(
                name: "IX_AIGenerations_TenantId_Status_CreatedAtUtc",
                table: "AIGenerations");
        }
    }
}
