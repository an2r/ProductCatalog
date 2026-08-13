using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApiApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.category_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_category_name",
                table: "Categories",
                column: "category_name",
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO "Categories" (category_name)
                SELECT DISTINCT product_cathegory
                FROM product_catalog
                WHERE product_cathegory IS NOT NULL
                  AND TRIM(product_cathegory) <> '';
                """);

            migrationBuilder.AddColumn<int>(
                name: "product_cathegory_id",
                table: "product_catalog",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE product_catalog pc
                SET product_cathegory_id = c.category_id
                FROM "Categories" c
                WHERE pc.product_cathegory = c.category_name;
                """);

            migrationBuilder.DropColumn(
                name: "product_cathegory",
                table: "product_catalog");

            migrationBuilder.RenameColumn(
                name: "product_cathegory_id",
                table: "product_catalog",
                newName: "product_cathegory");

            migrationBuilder.CreateIndex(
                name: "IX_product_catalog_product_cathegory",
                table: "product_catalog",
                column: "product_cathegory");

            migrationBuilder.AddForeignKey(
                name: "FK_product_catalog_Categories_product_cathegory",
                table: "product_catalog",
                column: "product_cathegory",
                principalTable: "Categories",
                principalColumn: "category_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_catalog_Categories_product_cathegory",
                table: "product_catalog");

            migrationBuilder.DropIndex(
                name: "IX_product_catalog_product_cathegory",
                table: "product_catalog");

            migrationBuilder.RenameColumn(
                name: "product_cathegory",
                table: "product_catalog",
                newName: "product_cathegory_id");

            migrationBuilder.AddColumn<string>(
                name: "product_cathegory",
                table: "product_catalog",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE product_catalog pc
                SET product_cathegory = c.category_name
                FROM "Categories" c
                WHERE pc.product_cathegory_id = c.category_id;
                """);

            migrationBuilder.DropColumn(
                name: "product_cathegory_id",
                table: "product_catalog");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
