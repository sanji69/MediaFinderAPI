using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaFinder.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCommentAndAddReporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Comments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HiddenAt",
                table: "Comments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaPosterPath",
                table: "Comments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaTitle",
                table: "Comments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ModeratedByUserId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "CommentReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReporterUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentReports_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentReports_Users_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_MediaType_MediaId",
                table: "Comments",
                columns: new[] { "MediaType", "MediaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Status",
                table: "Comments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReports_CommentId_ReporterUserId",
                table: "CommentReports",
                columns: new[] { "CommentId", "ReporterUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentReports_CreatedAt",
                table: "CommentReports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReports_ReporterUserId",
                table: "CommentReports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReports_Status",
                table: "CommentReports",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentReports");

            migrationBuilder.DropIndex(
                name: "IX_Comments_MediaType_MediaId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_Status",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "HiddenAt",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "MediaPosterPath",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "MediaTitle",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ModeratedByUserId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Comments");
        }
    }
}
