using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSDL.Migrations
{
    public partial class addUniqueAccountIdInUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_accountId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_accountId",
                table: "Users",
                column: "accountId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_accountId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_accountId",
                table: "Users",
                column: "accountId");
        }
    }
}
