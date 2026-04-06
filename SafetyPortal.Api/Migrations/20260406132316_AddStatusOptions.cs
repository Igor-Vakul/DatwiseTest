using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafetyPortal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActionStatusOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionStatusOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentStatusOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsClosing = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentStatusOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeverityLevelOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeverityLevelOptions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ActionStatusOptions",
                columns: new[] { "Id", "DisplayOrder", "IsActive", "IsCompleted", "IsSystem", "Name" },
                values: new object[,]
                {
                    { 1, 1, true, false, true, "Pending" },
                    { 2, 2, true, false, true, "InProgress" },
                    { 3, 3, true, true, true, "Completed" }
                });

            migrationBuilder.InsertData(
                table: "IncidentStatusOptions",
                columns: new[] { "Id", "DisplayOrder", "IsActive", "IsClosing", "IsSystem", "Name" },
                values: new object[,]
                {
                    { 1, 1, true, false, true, "Open" },
                    { 2, 2, true, false, true, "InProgress" },
                    { 3, 3, true, true, true, "Closed" }
                });

            migrationBuilder.InsertData(
                table: "SeverityLevelOptions",
                columns: new[] { "Id", "DisplayOrder", "IsActive", "IsSystem", "Name" },
                values: new object[,]
                {
                    { 1, 1, true, true, "Low" },
                    { 2, 2, true, true, "Medium" },
                    { 3, 3, true, true, "High" },
                    { 4, 4, true, true, "Critical" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionStatusOptions_Name",
                table: "ActionStatusOptions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentStatusOptions_Name",
                table: "IncidentStatusOptions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeverityLevelOptions_Name",
                table: "SeverityLevelOptions",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionStatusOptions");

            migrationBuilder.DropTable(
                name: "IncidentStatusOptions");

            migrationBuilder.DropTable(
                name: "SeverityLevelOptions");
        }
    }
}
