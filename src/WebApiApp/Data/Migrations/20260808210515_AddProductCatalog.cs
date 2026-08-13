using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApiApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_catalog",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    product_cathegory = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    product_description = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    product_cost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    product_note = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    product_note_special = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_catalog", x => x.product_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_catalog");
        }
    }
}
