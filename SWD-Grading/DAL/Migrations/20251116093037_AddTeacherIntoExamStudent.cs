using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherIntoExamStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeacherCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "exam_student",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_exam_student_TeacherId",
                table: "exam_student",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_exam_student_Users_TeacherId",
                table: "exam_student",
                column: "TeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exam_student_Users_TeacherId",
                table: "exam_student");

            migrationBuilder.DropIndex(
                name: "IX_exam_student_TeacherId",
                table: "exam_student");

            migrationBuilder.DropColumn(
                name: "TeacherCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "exam_student");
        }
    }
}
