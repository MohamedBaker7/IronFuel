using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IronFuel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIndexOfSKUcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_WeightG_SKU",
                table: "ProductVariants");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_WeightG",
                table: "ProductVariants",
                columns: new[] { "ProductId", "FlavourId", "WeightG" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_SKU",
                table: "ProductVariants",
                column: "SKU",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_WeightG",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_SKU",
                table: "ProductVariants");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_WeightG_SKU",
                table: "ProductVariants",
                columns: new[] { "ProductId", "FlavourId", "WeightG", "SKU" },
                unique: true);
        }
    }
}
