using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPlagiarismDetection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "similarity_check",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<long>(type: "bigint", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Threshold = table.Column<decimal>(type: "DECIMAL(5,4)", nullable: false),
                    CheckedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_similarity_check", x => x.Id);
                    table.ForeignKey(
                        name: "FK_similarity_check_Users_CheckedByUserId",
                        column: x => x.CheckedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_similarity_check_exam_ExamId",
                        column: x => x.ExamId,
                        principalTable: "exam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "similarity_result",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SimilarityCheckId = table.Column<long>(type: "bigint", nullable: false),
                    DocFile1Id = table.Column<long>(type: "bigint", nullable: false),
                    DocFile2Id = table.Column<long>(type: "bigint", nullable: false),
                    SimilarityScore = table.Column<decimal>(type: "DECIMAL(5,4)", nullable: false),
                    Student1Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Student2Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_similarity_result", x => x.Id);
                    table.ForeignKey(
                        name: "FK_similarity_result_doc_file_DocFile1Id",
                        column: x => x.DocFile1Id,
                        principalTable: "doc_file",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_similarity_result_doc_file_DocFile2Id",
                        column: x => x.DocFile2Id,
                        principalTable: "doc_file",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_similarity_result_similarity_check_SimilarityCheckId",
                        column: x => x.SimilarityCheckId,
                        principalTable: "similarity_check",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_similarity_check_CheckedByUserId",
                table: "similarity_check",
                column: "CheckedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_similarity_check_ExamId",
                table: "similarity_check",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_similarity_result_DocFile1Id",
                table: "similarity_result",
                column: "DocFile1Id");

            migrationBuilder.CreateIndex(
                name: "IX_similarity_result_DocFile2Id",
                table: "similarity_result",
                column: "DocFile2Id");

            migrationBuilder.CreateIndex(
                name: "IX_similarity_result_SimilarityCheckId",
                table: "similarity_result",
                column: "SimilarityCheckId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "similarity_result");

            migrationBuilder.DropTable(
                name: "similarity_check");
        }
    }
}
