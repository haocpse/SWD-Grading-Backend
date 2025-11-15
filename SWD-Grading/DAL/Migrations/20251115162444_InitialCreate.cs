using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "student",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClassName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "exam_question",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<long>(type: "bigint", nullable: false),
                    QuestionNumber = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "TEXT", nullable: true),
                    MaxScore = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    RelatedDocSection = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_question", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_question_exam_ExamId",
                        column: x => x.ExamId,
                        principalTable: "exam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_zip",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<long>(type: "bigint", nullable: false),
                    ZipName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ZipPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    ExtractedPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParseStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ParseSummary = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_zip", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_zip_exam_ExamId",
                        column: x => x.ExamId,
                        principalTable: "exam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_student",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<long>(type: "bigint", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_student", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_student_exam_ExamId",
                        column: x => x.ExamId,
                        principalTable: "exam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exam_student_student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "student",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rubric",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamQuestionId = table.Column<long>(type: "bigint", nullable: false),
                    Criterion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MaxScore = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    AutoCheckRule = table.Column<string>(type: "TEXT", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rubric", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rubric_exam_question_ExamQuestionId",
                        column: x => x.ExamQuestionId,
                        principalTable: "exam_question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "doc_file",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamStudentId = table.Column<long>(type: "bigint", nullable: false),
                    ExamZipId = table.Column<long>(type: "bigint", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParsedText = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    ParseStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ParseMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doc_file", x => x.Id);
                    table.ForeignKey(
                        name: "FK_doc_file_exam_student_ExamStudentId",
                        column: x => x.ExamStudentId,
                        principalTable: "exam_student",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_doc_file_exam_zip_ExamZipId",
                        column: x => x.ExamZipId,
                        principalTable: "exam_zip",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grade",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamStudentId = table.Column<long>(type: "bigint", nullable: false),
                    TotalScore = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: true),
                    GradedAt = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    GradedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_grade_exam_student_ExamStudentId",
                        column: x => x.ExamStudentId,
                        principalTable: "exam_student",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grade_detail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeId = table.Column<long>(type: "bigint", nullable: false),
                    RubricId = table.Column<long>(type: "bigint", nullable: false),
                    Score = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: true),
                    AutoDetectResult = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade_detail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_grade_detail_grade_GradeId",
                        column: x => x.GradeId,
                        principalTable: "grade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_grade_detail_rubric_RubricId",
                        column: x => x.RubricId,
                        principalTable: "rubric",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_doc_file_ExamStudentId",
                table: "doc_file",
                column: "ExamStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_doc_file_ExamZipId",
                table: "doc_file",
                column: "ExamZipId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_ExamCode",
                table: "exam",
                column: "ExamCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_question_ExamId",
                table: "exam_question",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_student_ExamId",
                table: "exam_student",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_student_StudentId",
                table: "exam_student",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_zip_ExamId",
                table: "exam_zip",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_grade_ExamStudentId",
                table: "grade",
                column: "ExamStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_grade_detail_GradeId",
                table: "grade_detail",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_grade_detail_RubricId",
                table: "grade_detail",
                column: "RubricId");

            migrationBuilder.CreateIndex(
                name: "IX_rubric_ExamQuestionId",
                table: "rubric",
                column: "ExamQuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "doc_file");

            migrationBuilder.DropTable(
                name: "grade_detail");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "exam_zip");

            migrationBuilder.DropTable(
                name: "grade");

            migrationBuilder.DropTable(
                name: "rubric");

            migrationBuilder.DropTable(
                name: "exam_student");

            migrationBuilder.DropTable(
                name: "exam_question");

            migrationBuilder.DropTable(
                name: "student");

            migrationBuilder.DropTable(
                name: "exam");
        }
    }
}
