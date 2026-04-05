using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IronFuel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlterProductVariantsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_Flavors_FlavourId",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_Size",
                table: "ProductVariants");

            migrationBuilder.AlterColumn<int>(
                name: "FlavourId",
                table: "ProductVariants",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_Size",
                table: "ProductVariants",
                columns: new[] { "ProductId", "FlavourId", "Size" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_Flavors_FlavourId",
                table: "ProductVariants",
                column: "FlavourId",
                principalTable: "Flavors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_Flavors_FlavourId",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_Size",
                table: "ProductVariants");

            migrationBuilder.AlterColumn<int>(
                name: "FlavourId",
                table: "ProductVariants",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_Size",
                table: "ProductVariants",
                columns: new[] { "ProductId", "FlavourId", "Size" },
                unique: true,
                filter: "[FlavourId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_Flavors_FlavourId",
                table: "ProductVariants",
                column: "FlavourId",
                principalTable: "Flavors",
                principalColumn: "Id");
        }
    }
}
