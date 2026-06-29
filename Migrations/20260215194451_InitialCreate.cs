using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AustimAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Question",
                columns: table => new
                {
                    QuestionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionNumber = table.Column<int>(type: "int", nullable: false),
                    QuestionText_AR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionText_EN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskIfNo = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Question", x => x.QuestionID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Child",
                columns: table => new
                {
                    ChildID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    ChildName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Child", x => x.ChildID);
                    table.ForeignKey(
                        name: "FK_Child_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Screening",
                columns: table => new
                {
                    ScreeningID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChildID = table.Column<int>(type: "int", nullable: false),
                    ScreeningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: true),
                    RiskLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Screening", x => x.ScreeningID);
                    table.ForeignKey(
                        name: "FK_Screening_Child_ChildID",
                        column: x => x.ChildID,
                        principalTable: "Child",
                        principalColumn: "ChildID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIResult",
                columns: table => new
                {
                    ResultID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScreeningID = table.Column<int>(type: "int", nullable: false),
                    VideoPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EyeContactScore = table.Column<float>(type: "real", nullable: true),
                    FaceEmotionScore = table.Column<float>(type: "real", nullable: true),
                    AI_JSON_Data = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIResult", x => x.ResultID);
                    table.ForeignKey(
                        name: "FK_AIResult_Screening_ScreeningID",
                        column: x => x.ScreeningID,
                        principalTable: "Screening",
                        principalColumn: "ScreeningID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireAnswer",
                columns: table => new
                {
                    AnswerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScreeningID = table.Column<int>(type: "int", nullable: false),
                    QuestionID = table.Column<int>(type: "int", nullable: false),
                    AnswerValue = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireAnswer", x => x.AnswerID);
                    table.ForeignKey(
                        name: "FK_QuestionnaireAnswer_Question_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "Question",
                        principalColumn: "QuestionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionnaireAnswer_Screening_ScreeningID",
                        column: x => x.ScreeningID,
                        principalTable: "Screening",
                        principalColumn: "ScreeningID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIResult_ScreeningID",
                table: "AIResult",
                column: "ScreeningID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Child_UserID",
                table: "Child",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAnswer_QuestionID",
                table: "QuestionnaireAnswer",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAnswer_ScreeningID",
                table: "QuestionnaireAnswer",
                column: "ScreeningID");

            migrationBuilder.CreateIndex(
                name: "IX_Screening_ChildID",
                table: "Screening",
                column: "ChildID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIResult");

            migrationBuilder.DropTable(
                name: "QuestionnaireAnswer");

            migrationBuilder.DropTable(
                name: "Question");

            migrationBuilder.DropTable(
                name: "Screening");

            migrationBuilder.DropTable(
                name: "Child");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
