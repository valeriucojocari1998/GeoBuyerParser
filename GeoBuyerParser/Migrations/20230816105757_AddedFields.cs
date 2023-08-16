using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoBuyerParser.Migrations
{
    /// <inheritdoc />
    public partial class AddedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "imageUrl",
                table: "Spots",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "url",
                table: "Spots",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "categoryName",
                table: "Products",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "categoryId",
                table: "Products",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imageUrl",
                table: "Spots");

            migrationBuilder.DropColumn(
                name: "url",
                table: "Spots");

            migrationBuilder.AlterColumn<string>(
                name: "categoryName",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "categoryId",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
