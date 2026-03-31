using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyPortal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "IncidentReports",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "IncidentReports");
        }
    }
}
