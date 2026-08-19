using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateSequence(
                name: "catalog_brand_hilo",
                schema: "catalog",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "catalog_hilo",
                schema: "catalog",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "catalog_type_hilo",
                schema: "catalog",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "CatalogBrand",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogBrand", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogType",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogItem",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: false),
                    PictureFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PictureUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CatalogTypeId = table.Column<int>(type: "int", nullable: false),
                    CatalogBrandId = table.Column<int>(type: "int", nullable: false),
                    AvailableStock = table.Column<int>(type: "int", nullable: false),
                    RestockThreshold = table.Column<int>(type: "int", nullable: false),
                    MaxStockThreshold = table.Column<int>(type: "int", nullable: false),
                    OnReorder = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogItem_CatalogBrand_CatalogBrandId",
                        column: x => x.CatalogBrandId,
                        principalSchema: "catalog",
                        principalTable: "CatalogBrand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogItem_CatalogType_CatalogTypeId",
                        column: x => x.CatalogTypeId,
                        principalSchema: "catalog",
                        principalTable: "CatalogType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogBrand_Brand",
                schema: "catalog",
                table: "CatalogBrand",
                column: "Brand",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItem_CatalogBrandId_CatalogTypeId",
                schema: "catalog",
                table: "CatalogItem",
                columns: new[] { "CatalogBrandId", "CatalogTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItem_CatalogTypeId",
                schema: "catalog",
                table: "CatalogItem",
                column: "CatalogTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItem_Name",
                schema: "catalog",
                table: "CatalogItem",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogType_Type",
                schema: "catalog",
                table: "CatalogType",
                column: "Type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogItem",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CatalogBrand",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CatalogType",
                schema: "catalog");

            migrationBuilder.DropSequence(
                name: "catalog_brand_hilo",
                schema: "catalog");

            migrationBuilder.DropSequence(
                name: "catalog_hilo",
                schema: "catalog");

            migrationBuilder.DropSequence(
                name: "catalog_type_hilo",
                schema: "catalog");
        }
    }
}
