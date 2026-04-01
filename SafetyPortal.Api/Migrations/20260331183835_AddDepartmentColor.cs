using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyPortal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Departments",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#6c757d");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "Color",
                value: "#6c757d");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "Color",
                value: "#6c757d");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "Color",
                value: "#6c757d");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "Color",
                value: "#6c757d");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "Color",
                value: "#6c757d");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Departments");
        }
    }
}
