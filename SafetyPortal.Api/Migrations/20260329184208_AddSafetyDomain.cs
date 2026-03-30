using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafetyPortal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSafetyDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    ReportedByUserId = table.Column<int>(type: "int", nullable: false),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                    IncidentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocationDetails = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SeverityLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentReports_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentReports_IncidentCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "IncidentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentReports_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentReports_Users_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CorrectiveActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    ActionTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ActionDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PriorityLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrectiveActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorrectiveActions_IncidentReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "IncidentReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CorrectiveActions_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "IsActive", "LocationName", "Name" },
                values: new object[,]
                {
                    { 1, true, "Plant A", "Production" },
                    { 2, true, "Plant A", "Warehouse" },
                    { 3, true, "Plant B", "Maintenance" },
                    { 4, true, "Plant B", "Logistics" },
                    { 5, true, "HQ", "Quality Assurance" }
                });

            migrationBuilder.InsertData(
                table: "IncidentCategories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Event that could have caused injury or damage but did not", "Near Miss" },
                    { 2, "Identified workplace hazard", "Hazard" },
                    { 3, "Unsafe physical condition in workplace", "Unsafe Condition" },
                    { 4, "Unsafe employee action or behavior", "Unsafe Act" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_AssignedToUserId",
                table: "CorrectiveActions",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_ReportId",
                table: "CorrectiveActions",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCategories_Name",
                table: "IncidentCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_AssignedToUserId",
                table: "IncidentReports",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_CategoryId",
                table: "IncidentReports",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_DepartmentId",
                table: "IncidentReports",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_ReportedByUserId",
                table: "IncidentReports",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_ReportNumber",
                table: "IncidentReports",
                column: "ReportNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorrectiveActions");

            migrationBuilder.DropTable(
                name: "IncidentReports");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "IncidentCategories");
        }
    }
}
