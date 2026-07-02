using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaFinder.Migrations
{
    /// <inheritdoc />
    public partial class AdminRoleAndUserStatusUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BannedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannedAt",
                table: "Users");
        }
    }
}
