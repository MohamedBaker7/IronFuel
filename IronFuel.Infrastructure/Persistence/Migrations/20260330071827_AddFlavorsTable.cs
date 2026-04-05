using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IronFuel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFlavorsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Flavour",
                table: "ProductVariants");

            migrationBuilder.AddColumn<int>(
                name: "FlavourId",
                table: "ProductVariants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ServingWeight",
                table: "ProductVariants",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Flavors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastUpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "NULL")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flavors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_FlavourId",
                table: "ProductVariants",
                column: "FlavourId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_Flavors_FlavourId",
                table: "ProductVariants");

            migrationBuilder.DropTable(
                name: "Flavors");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_FlavourId",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId_FlavourId_Size",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "FlavourId",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ServingWeight",
                table: "ProductVariants");

            migrationBuilder.AddColumn<string>(
                name: "Flavour",
                table: "ProductVariants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");
        }
    }
}
