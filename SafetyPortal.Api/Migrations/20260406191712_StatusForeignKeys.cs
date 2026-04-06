using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyPortal.Api.Migrations
{
    /// <inheritdoc />
    public partial class StatusForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add new FK columns as nullable first (so existing rows are accepted)
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "IncidentReports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeverityLevelId",
                table: "IncidentReports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "CorrectiveActions",
                type: "int",
                nullable: true);

            // 2. Data-migrate: populate FK ids from the existing name strings
            migrationBuilder.Sql(@"
                UPDATE ir
                SET ir.StatusId = iso.Id
                FROM IncidentReports ir
                JOIN IncidentStatusOptions iso ON iso.Name = ir.Status;
            ");

            migrationBuilder.Sql(@"
                UPDATE ir
                SET ir.SeverityLevelId = slo.Id
                FROM IncidentReports ir
                JOIN SeverityLevelOptions slo ON slo.Name = ir.SeverityLevel;
            ");

            migrationBuilder.Sql(@"
                UPDATE ca
                SET ca.StatusId = aso.Id
                FROM CorrectiveActions ca
                JOIN ActionStatusOptions aso ON aso.Name = ca.Status;
            ");

            // Fallback: any unmatched rows get the first option (Id=1)
            migrationBuilder.Sql("UPDATE IncidentReports SET StatusId = 1 WHERE StatusId IS NULL;");
            migrationBuilder.Sql("UPDATE IncidentReports SET SeverityLevelId = 1 WHERE SeverityLevelId IS NULL;");
            migrationBuilder.Sql("UPDATE CorrectiveActions SET StatusId = 1 WHERE StatusId IS NULL;");

            // 3. Now make columns non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "StatusId",
                table: "IncidentReports",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SeverityLevelId",
                table: "IncidentReports",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StatusId",
                table: "CorrectiveActions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            // 4. Drop the old string columns
            migrationBuilder.DropColumn(name: "Status",        table: "IncidentReports");
            migrationBuilder.DropColumn(name: "SeverityLevel", table: "IncidentReports");
            migrationBuilder.DropColumn(name: "Status",        table: "CorrectiveActions");

            // 5. Add indexes and FK constraints
            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_StatusId",
                table: "IncidentReports",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_SeverityLevelId",
                table: "IncidentReports",
                column: "SeverityLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_StatusId",
                table: "CorrectiveActions",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentReports_IncidentStatusOptions_StatusId",
                table: "IncidentReports",
                column: "StatusId",
                principalTable: "IncidentStatusOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentReports_SeverityLevelOptions_SeverityLevelId",
                table: "IncidentReports",
                column: "SeverityLevelId",
                principalTable: "SeverityLevelOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CorrectiveActions_ActionStatusOptions_StatusId",
                table: "CorrectiveActions",
                column: "StatusId",
                principalTable: "ActionStatusOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CorrectiveActions_ActionStatusOptions_StatusId",
                table: "CorrectiveActions");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentReports_IncidentStatusOptions_StatusId",
                table: "IncidentReports");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentReports_SeverityLevelOptions_SeverityLevelId",
                table: "IncidentReports");

            migrationBuilder.DropIndex(name: "IX_IncidentReports_SeverityLevelId", table: "IncidentReports");
            migrationBuilder.DropIndex(name: "IX_IncidentReports_StatusId",        table: "IncidentReports");
            migrationBuilder.DropIndex(name: "IX_CorrectiveActions_StatusId",       table: "CorrectiveActions");

            // Restore string columns
            migrationBuilder.AddColumn<string>(
                name: "SeverityLevel",
                table: "IncidentReports",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "IncidentReports",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CorrectiveActions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            // Data-migrate back
            migrationBuilder.Sql(@"
                UPDATE ir SET ir.Status = iso.Name
                FROM IncidentReports ir JOIN IncidentStatusOptions iso ON iso.Id = ir.StatusId;
            ");
            migrationBuilder.Sql(@"
                UPDATE ir SET ir.SeverityLevel = slo.Name
                FROM IncidentReports ir JOIN SeverityLevelOptions slo ON slo.Id = ir.SeverityLevelId;
            ");
            migrationBuilder.Sql(@"
                UPDATE ca SET ca.Status = aso.Name
                FROM CorrectiveActions ca JOIN ActionStatusOptions aso ON aso.Id = ca.StatusId;
            ");

            migrationBuilder.DropColumn(name: "SeverityLevelId", table: "IncidentReports");
            migrationBuilder.DropColumn(name: "StatusId",         table: "IncidentReports");
            migrationBuilder.DropColumn(name: "StatusId",         table: "CorrectiveActions");
        }
    }
}
