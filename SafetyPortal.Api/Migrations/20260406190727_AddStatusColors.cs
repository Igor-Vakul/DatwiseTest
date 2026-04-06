using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyPortal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusColors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "SeverityLevelOptions",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#6c757d");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "IncidentStatusOptions",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#6c757d");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "ActionStatusOptions",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#6c757d");

            migrationBuilder.UpdateData(
                table: "ActionStatusOptions",
                keyColumn: "Id",
                keyValue: 1,
                column: "Color",
                value: "#6c757d");

            migrationBuilder.UpdateData(
                table: "ActionStatusOptions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Color",
                value: "#0d6efd");

            migrationBuilder.UpdateData(
                table: "ActionStatusOptions",
                keyColumn: "Id",
                keyValue: 3,
                column: "Color",
                value: "#198754");

            migrationBuilder.UpdateData(
                table: "IncidentStatusOptions",
                keyColumn: "Id",
                keyValue: 1,
                column: "Color",
                value: "#0d6efd");

            migrationBuilder.UpdateData(
                table: "IncidentStatusOptions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Color",
                value: "#6610f2");

            migrationBuilder.UpdateData(
                table: "IncidentStatusOptions",
                keyColumn: "Id",
                keyValue: 3,
                column: "Color",
                value: "#198754");

            migrationBuilder.UpdateData(
                table: "SeverityLevelOptions",
                keyColumn: "Id",
                keyValue: 1,
                column: "Color",
                value: "#198754");

            migrationBuilder.UpdateData(
                table: "SeverityLevelOptions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Color",
                value: "#ffc107");

            migrationBuilder.UpdateData(
                table: "SeverityLevelOptions",
                keyColumn: "Id",
                keyValue: 3,
                column: "Color",
                value: "#fd7e14");

            migrationBuilder.UpdateData(
                table: "SeverityLevelOptions",
                keyColumn: "Id",
                keyValue: 4,
                column: "Color",
                value: "#dc3545");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "SeverityLevelOptions");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "IncidentStatusOptions");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "ActionStatusOptions");
        }
    }
}
