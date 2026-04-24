using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IronFuel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsToProductVariantsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_Size",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ServingWeight",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "ProductVariants");

            migrationBuilder.RenameColumn(
                name: "StockQuantity",
                table: "ProductVariants",
                newName: "WeightG");

            migrationBuilder.AddColumn<int>(
                name: "ServingSizeG",
                table: "ProductVariants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "ProductVariants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ServingsPerContainer",
                table: "ProductVariants",
                type: "int",
                nullable: false,
                computedColumnSql: "(WeightG / NULLIF(ServingSizeG, 0))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_WeightG",
                table: "ProductVariants",
                columns: new[] { "ProductId", "FlavourId", "WeightG" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_WeightG",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ServingsPerContainer",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ServingSizeG",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "ProductVariants");

            migrationBuilder.RenameColumn(
                name: "WeightG",
                table: "ProductVariants",
                newName: "StockQuantity");

            migrationBuilder.AddColumn<decimal>(
                name: "ServingWeight",
                table: "ProductVariants",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Size",
                table: "ProductVariants",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_Size",
                table: "ProductVariants",
                columns: new[] { "ProductId", "FlavourId", "Size" },
                unique: true);
        }
    }
}
