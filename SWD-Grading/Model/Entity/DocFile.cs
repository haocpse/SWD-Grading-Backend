using Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
	[Table("DocFile")]
	public class DocFile
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		// FK → ExamStudent.id
		[Required]
		public long ExamStudentId { get; set; }

		[ForeignKey(nameof(ExamStudentId))]
		public ExamStudent ExamStudent { get; set; } = null!;

		// FK → ExamUpload.id (ZIP mà file được giải nén)
		[Required]
		public long ExamZipId { get; set; }

		[ForeignKey(nameof(ExamZipId))]
		public ExamZip ExamZip { get; set; } = null!;

		[MaxLength(255)]
		public string? FileName { get; set; }

		[MaxLength(500)]
		public string? FilePath { get; set; }

		public string? ParsedText { get; set; } // LONGTEXT

	[Required]
	public DocParseStatus ParseStatus { get; set; } = DocParseStatus.NOT_FOUND;

	public string? ParseMessage { get; set; }

	/// <summary>
	/// Indicates whether this document has been embedded into the vector database.
	/// False = not yet embedded (will be automatically processed by background job)
	/// True = already embedded
	/// </summary>
	[Required]
	public bool IsEmbedded { get; set; } = false;
}
}
