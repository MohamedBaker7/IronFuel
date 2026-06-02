using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IronFuel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeColumnToBrandsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Brands",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Brands");
        }
    }
}
