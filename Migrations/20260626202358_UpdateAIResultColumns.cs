using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AustimAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAIResultColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FaceEmotionScore",
                table: "AIResult",
                newName: "RiskScorePercentage");

            migrationBuilder.RenameColumn(
                name: "EyeContactScore",
                table: "AIResult",
                newName: "OverallConfidence");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RiskScorePercentage",
                table: "AIResult",
                newName: "FaceEmotionScore");

            migrationBuilder.RenameColumn(
                name: "OverallConfidence",
                table: "AIResult",
                newName: "EyeContactScore");
        }
    }
}
