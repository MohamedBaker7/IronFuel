using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IronFuel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSKUpropertyToProductVariantTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_WeightG",
                table: "ProductVariants");

            migrationBuilder.AddColumn<string>(
                name: "SKU",
                table: "ProductVariants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Products",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Flavors",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_WeightG_SKU",
                table: "ProductVariants",
                columns: new[] { "ProductId", "FlavourId", "WeightG", "SKU" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_WeightG_SKU",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "SKU",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Flavors");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_WeightG",
                table: "ProductVariants",
                columns: new[] { "ProductId", "FlavourId", "WeightG" },
                unique: true);
        }
    }
}
