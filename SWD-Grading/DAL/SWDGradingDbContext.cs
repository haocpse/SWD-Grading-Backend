using Microsoft.EntityFrameworkCore;
using Model.Entity;

namespace DAL
{
	public class SWDGradingDbContext : DbContext
	{
		public SWDGradingDbContext(DbContextOptions<SWDGradingDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }

		public DbSet<Exam> Exams { get; set; }
		public DbSet<Student> Students { get; set; }
		public DbSet<ExamStudent> ExamStudents { get; set; }
		public DbSet<ExamZip> ExamZips { get; set; }
		public DbSet<DocFile> DocFiles { get; set; }
		public DbSet<ExamQuestion> ExamQuestions { get; set; }
		public DbSet<Rubric> Rubrics { get; set; }
		public DbSet<Grade> Grades { get; set; }
		public DbSet<GradeDetail> GradeDetails { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Exam>(entity =>
			{
				entity.ToTable("exam");

				entity.HasKey(e => e.Id);

				entity.Property(e => e.ExamCode)
					  .HasMaxLength(50)
					  .IsRequired();

				entity.HasIndex(e => e.ExamCode)
					  .IsUnique();

				entity.Property(e => e.Title)
					  .HasMaxLength(255);

				entity.Property(e => e.Description)
					  .HasColumnType("TEXT");

				entity.Property(e => e.CreatedAt)
					  .HasColumnType("DATETIME");

				entity.Property(e => e.UpdatedAt)
					  .HasColumnType("DATETIME");
			});
			modelBuilder.Entity<Student>(entity =>
			{
				entity.ToTable("student");

				entity.HasKey(e => e.Id);

				entity.Property(e => e.StudentCode)
					  .HasMaxLength(50)
					  .IsRequired();

				entity.Property(e => e.FullName)
					  .HasMaxLength(255);

				entity.Property(e => e.Email)
					  .HasMaxLength(100);

				entity.Property(e => e.ClassName)
					  .HasMaxLength(100);
			});
			modelBuilder.Entity<ExamStudent>(entity =>
			{
				entity.ToTable("exam_student");

				entity.HasKey(e => e.Id);

				entity.Property(e => e.ExamId).IsRequired();
				entity.Property(e => e.StudentId).IsRequired();

				// ENUM mapping → string
				entity.Property(e => e.Status)
					  .HasConversion<string>()
					  .HasMaxLength(20);

				entity.Property(e => e.Note)
					  .HasColumnType("TEXT");

				entity.HasOne(e => e.Exam)
					  .WithMany()
					  .HasForeignKey(e => e.ExamId)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(e => e.Student)
					  .WithMany()
					  .HasForeignKey(e => e.StudentId)
					  .OnDelete(DeleteBehavior.Cascade);
				entity.HasOne(e => e.Teacher)
					  .WithMany()
					  .HasForeignKey(e => e.TeacherId)
					  .OnDelete(DeleteBehavior.Cascade);
			});
			modelBuilder.Entity<ExamZip>(entity =>
			{
				entity.ToTable("exam_zip");

				entity.HasKey(e => e.Id);

				entity.Property(e => e.ExamId).IsRequired();

				entity.Property(e => e.ZipName)
					  .HasMaxLength(255);

				entity.Property(e => e.ZipPath)
					  .HasMaxLength(500);

				entity.Property(e => e.UploadedAt)
					  .HasColumnType("DATETIME");

				entity.Property(e => e.ExtractedPath)
					  .HasMaxLength(500);

				// ENUM → string
				entity.Property(e => e.ParseStatus)
					  .HasConversion<string>()
					  .HasMaxLength(20)
					  .IsRequired();

				entity.Property(e => e.ParseSummary)
					  .HasColumnType("TEXT");

				entity.HasOne(e => e.Exam)
					  .WithMany()
					  .HasForeignKey(e => e.ExamId)
					  .OnDelete(DeleteBehavior.Cascade);
			});
			modelBuilder.Entity<DocFile>(entity =>
			{
				entity.ToTable("doc_file");

				entity.HasKey(e => e.Id);

				entity.Property(e => e.FileName)
					  .HasMaxLength(255);

				entity.Property(e => e.FilePath)
					  .HasMaxLength(500);

				entity.Property(e => e.ParsedText)
					  .HasColumnType("NVARCHAR(MAX)");

				entity.Property(e => e.ParseMessage)
					  .HasColumnType("TEXT");

				// ENUM → string
				entity.Property(e => e.ParseStatus)
					  .HasConversion<string>()
					  .HasMaxLength(20)
					  .IsRequired();

				entity.HasOne(e => e.ExamStudent)
					  .WithMany()
					  .HasForeignKey(e => e.ExamStudentId)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(e => e.ExamZip)
					  .WithMany()
	                  .HasForeignKey(e => e.ExamZipId)
					  .OnDelete(DeleteBehavior.Restrict);
			});
			modelBuilder.Entity<ExamQuestion>(entity =>
			{
				entity.ToTable("exam_question");

				entity.HasKey(e => e.Id);

				entity.Property(e => e.QuestionNumber)
					  .IsRequired();

				entity.Property(e => e.QuestionText)
					  .HasColumnType("TEXT");

				entity.Property(e => e.MaxScore)
					  .HasColumnType("DECIMAL(5,2)");

				entity.Property(e => e.RelatedDocSection)
					  .HasMaxLength(255);

				entity.HasOne(e => e.Exam)
					  .WithMany()
					  .HasForeignKey(e => e.ExamId)
					  .OnDelete(DeleteBehavior.Cascade);
			});
			modelBuilder.Entity<Rubric>(entity =>
			{
				entity.ToTable("rubric");

				entity.HasKey(e => e.Id);

				entity.Property(e => e.Criterion)
					  .HasMaxLength(255)
					  .IsRequired();

				entity.Property(e => e.MaxScore)
					  .HasColumnType("DECIMAL(5,2)");

				entity.Property(e => e.AutoCheckRule)
					  .HasColumnType("TEXT"); // JSON

				entity.Property(e => e.OrderIndex)
					  .IsRequired();

				entity.HasOne(e => e.ExamQuestion)
					  .WithMany()
					  .HasForeignKey(e => e.ExamQuestionId)
					  .OnDelete(DeleteBehavior.Cascade);
			});
			modelBuilder.Entity<Grade>(entity =>
			{
				entity.ToTable("grade");

				entity.HasKey(e => e.Id);

				entity.Property(e => e.TotalScore)
					  .HasColumnType("DECIMAL(5,2)");

				entity.Property(e => e.Comment)
					  .HasColumnType("TEXT");

				entity.Property(e => e.GradedAt)
					  .HasColumnType("DATETIME");

				entity.Property(e => e.GradedBy)
					  .HasMaxLength(100);

				entity.HasOne(e => e.ExamStudent)
					  .WithMany()
					  .HasForeignKey(e => e.ExamStudentId)
					  .OnDelete(DeleteBehavior.Cascade);
			});
			modelBuilder.Entity<GradeDetail>(entity =>
			{
				entity.ToTable("grade_detail");

				entity.HasKey(e => e.Id);

				entity.Property(e => e.Score)
					  .HasColumnType("DECIMAL(5,2)");

				entity.Property(e => e.Comment)
					  .HasColumnType("TEXT");

				entity.Property(e => e.AutoDetectResult)
					  .HasColumnType("TEXT");

				entity.HasOne(e => e.Grade)
					  .WithMany()
					  .HasForeignKey(e => e.GradeId)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(e => e.Rubric)
					  .WithMany()
					  .HasForeignKey(e => e.RubricId)
					  .OnDelete(DeleteBehavior.Restrict);
			});
		}
	}
}
