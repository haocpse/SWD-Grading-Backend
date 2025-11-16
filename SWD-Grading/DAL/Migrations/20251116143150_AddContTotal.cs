using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddContTotal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AIVerificationResult",
                table: "similarity_result",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AIVerifiedAt",
                table: "similarity_result",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherNotes",
                table: "similarity_result",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TeacherVerifiedAt",
                table: "similarity_result",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeacherVerifiedByUserId",
                table: "similarity_result",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "similarity_result",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_similarity_result_TeacherVerifiedByUserId",
                table: "similarity_result",
                column: "TeacherVerifiedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_similarity_result_Users_TeacherVerifiedByUserId",
                table: "similarity_result",
                column: "TeacherVerifiedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_similarity_result_Users_TeacherVerifiedByUserId",
                table: "similarity_result");

            migrationBuilder.DropIndex(
                name: "IX_similarity_result_TeacherVerifiedByUserId",
                table: "similarity_result");

            migrationBuilder.DropColumn(
                name: "AIVerificationResult",
                table: "similarity_result");

            migrationBuilder.DropColumn(
                name: "AIVerifiedAt",
                table: "similarity_result");

            migrationBuilder.DropColumn(
                name: "TeacherNotes",
                table: "similarity_result");

            migrationBuilder.DropColumn(
                name: "TeacherVerifiedAt",
                table: "similarity_result");

            migrationBuilder.DropColumn(
                name: "TeacherVerifiedByUserId",
                table: "similarity_result");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "similarity_result");
        }
    }
}
