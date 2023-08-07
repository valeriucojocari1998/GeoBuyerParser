using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoBuyerParser.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    productCount = table.Column<int>(type: "INTEGER", nullable: false),
                    marketId = table.Column<string>(type: "TEXT", nullable: false),
                    marketProvider = table.Column<string>(type: "TEXT", nullable: false),
                    categoryUrl = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    currentPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    categoryId = table.Column<string>(type: "TEXT", nullable: false),
                    categoryName = table.Column<string>(type: "TEXT", nullable: false),
                    marketId = table.Column<string>(type: "TEXT", nullable: false),
                    marketProvider = table.Column<string>(type: "TEXT", nullable: false),
                    oldPrice = table.Column<decimal>(type: "TEXT", nullable: true),
                    brand = table.Column<string>(type: "TEXT", nullable: true),
                    priceLabel = table.Column<string>(type: "TEXT", nullable: true),
                    saleSpecification = table.Column<string>(type: "TEXT", nullable: true),
                    imageUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Spots",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    provider = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    latitude = table.Column<string>(type: "TEXT", nullable: true),
                    longitude = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spots", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Spots");
        }
    }
}
