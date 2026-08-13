using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncAuthModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_userRole",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "userLogin",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "passwordHash",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "roleName",
                table: "Roles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Users_userLogin",
                table: "Users",
                column: "userLogin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_roleName",
                table: "Roles",
                column: "roleName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_userRole",
                table: "Users",
                column: "userRole",
                principalTable: "Roles",
                principalColumn: "roleId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_userRole",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_userLogin",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Roles_roleName",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "passwordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "roleName",
                table: "Roles");

            migrationBuilder.AlterColumn<string>(
                name: "userLogin",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_userRole",
                table: "Users",
                column: "userRole",
                principalTable: "Roles",
                principalColumn: "roleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
