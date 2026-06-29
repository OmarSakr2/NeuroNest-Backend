using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AustimAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixGenderType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Child_User_UserID",
                table: "Child");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "Child",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Child",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)");

            migrationBuilder.AddForeignKey(
                name: "FK_Child_User_UserID",
                table: "Child",
                column: "UserID",
                principalTable: "User",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Child_User_UserID",
                table: "Child");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "Child",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Child",
                type: "nvarchar(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Child_User_UserID",
                table: "Child",
                column: "UserID",
                principalTable: "User",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
