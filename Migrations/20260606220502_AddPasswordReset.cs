using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AustimAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Child_User_UserID",
                table: "Child");

            migrationBuilder.DropIndex(
                name: "IX_Child_UserID",
                table: "Child");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Child");

            migrationBuilder.CreateTable(
                name: "PasswordReset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordReset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordReset_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Child_ParentID",
                table: "Child",
                column: "ParentID");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordReset_UserID",
                table: "PasswordReset",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Child_User_ParentID",
                table: "Child",
                column: "ParentID",
                principalTable: "User",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Child_User_ParentID",
                table: "Child");

            migrationBuilder.DropTable(
                name: "PasswordReset");

            migrationBuilder.DropIndex(
                name: "IX_Child_ParentID",
                table: "Child");

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Child",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Child_UserID",
                table: "Child",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Child_User_UserID",
                table: "Child",
                column: "UserID",
                principalTable: "User",
                principalColumn: "UserID");
        }
    }
}
