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
		public DbSet<SimilarityCheck> SimilarityChecks { get; set; }
		public DbSet<SimilarityResult> SimilarityResults { get; set; }
		public DbSet<GradeExport> GradeExports { get; set; }

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

				entity.HasOne(et => et.Exam)
					  .WithMany(e => e.ExamStudents)
					  .HasForeignKey(et => et.ExamId)
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
					  .WithMany(x => x.Questions)
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
					  .WithMany(q => q.Rubrics)
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
					  .HasColumnType("NVARCHAR(MAX)");

				entity.Property(e => e.GradedAt)
					  .HasColumnType("DATETIME");

				entity.Property(e => e.GradedBy)
					  .HasMaxLength(100);

				entity.Property(e => e.Attempt)
				.IsRequired();

				entity.Property(e => e.Status)
					  .HasConversion<int>();

				entity.HasOne(e => e.ExamStudent)
					  .WithMany(es => es.Grades)
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
					  .WithMany(g => g.Details)
					  .HasForeignKey(e => e.GradeId)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(e => e.Rubric)
					  .WithMany()
					  .HasForeignKey(e => e.RubricId)
					  .OnDelete(DeleteBehavior.Restrict);
			});
			modelBuilder.Entity<SimilarityCheck>(entity =>
			{
				entity.ToTable("similarity_check");

				entity.HasKey(e => e.Id);

				entity.Property(e => e.ExamId).IsRequired();

				entity.Property(e => e.CheckedAt)
					  .HasColumnType("DATETIME")
					  .IsRequired();

				entity.Property(e => e.Threshold)
					  .HasColumnType("DECIMAL(5,4)")
					  .IsRequired();

				entity.Property(e => e.CheckedByUserId).IsRequired();

				entity.HasOne(e => e.Exam)
					  .WithMany()
					  .HasForeignKey(e => e.ExamId)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(e => e.CheckedByUser)
					  .WithMany()
					  .HasForeignKey(e => e.CheckedByUserId)
					  .OnDelete(DeleteBehavior.Restrict);
			});
			modelBuilder.Entity<SimilarityResult>(entity =>
			{
				entity.ToTable("similarity_result");

				entity.HasKey(e => e.Id);

				entity.Property(e => e.SimilarityCheckId).IsRequired();
				entity.Property(e => e.DocFile1Id).IsRequired();
				entity.Property(e => e.DocFile2Id).IsRequired();

				entity.Property(e => e.SimilarityScore)
					  .HasColumnType("DECIMAL(5,4)")
					  .IsRequired();

				entity.Property(e => e.Student1Code)
					  .HasMaxLength(50);

				entity.Property(e => e.Student2Code)
					  .HasMaxLength(50);

				entity.HasOne(e => e.SimilarityCheck)
					  .WithMany(sc => sc.SimilarityResults)
					  .HasForeignKey(e => e.SimilarityCheckId)
					  .OnDelete(DeleteBehavior.Cascade);

				// Self-referencing relationships to DocFile
				entity.HasOne(e => e.DocFile1)
					  .WithMany()
					  .HasForeignKey(e => e.DocFile1Id)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(e => e.DocFile2)
					  .WithMany()
					  .HasForeignKey(e => e.DocFile2Id)
					  .OnDelete(DeleteBehavior.Restrict);
			});


			modelBuilder.Entity<GradeExport>(entity =>
			{
				entity.ToTable("GradeExport");

				// PK
				entity.HasKey(x => x.Id);

				// ExamId → Exam (1-N)
				entity.HasOne(x => x.Exam)
					.WithMany(e => e.GradeExports)
					.HasForeignKey(x => x.ExamId)
					.OnDelete(DeleteBehavior.Cascade);

				// UserId → User (Teacher) (1-N)
				entity.HasOne(x => x.Teacher)
					.WithMany(u => u.GradeExports)
					.HasForeignKey(x => x.UserId)
					.OnDelete(DeleteBehavior.Restrict);

				// CreatedAt default value
				entity.Property(x => x.CreatedAt)
					.HasDefaultValueSql("CURRENT_TIMESTAMP");

				// Url optional
				entity.Property(x => x.Url)
					.HasMaxLength(500);

				// IsFinal -> bool
				entity.Property(x => x.IsFinal)
					.HasDefaultValue(false);
			});
		}
	}
}