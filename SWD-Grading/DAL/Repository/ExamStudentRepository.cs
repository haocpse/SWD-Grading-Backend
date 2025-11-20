using DAL.Interface;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
	public class ExamStudentRepository : GenericRepository<ExamStudent, long>, IExamStudentRepository
	{
		private readonly SWDGradingDbContext _context;

		public ExamStudentRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task BulkInsertAsync(List<ExamStudent> examStudents)
		{
			await _context.Set<ExamStudent>().AddRangeAsync(examStudents);
		}

		public async Task<ExamStudent?> GetByExamAndStudentAsync(long examId, long studentId)
		{
			return await _context.Set<ExamStudent>()
				.Include(es => es.Student)
				.Include(es => es.Exam)
				.FirstOrDefaultAsync(es => es.ExamId == examId && es.StudentId == studentId);
		}

	public async Task<List<ExamStudent>> GetByExamZipIdAsync(long examZipId)
	{
		// Get ExamStudent records associated with a specific ExamZip through DocFile relationship
		return await _context.Set<ExamStudent>()
			.Include(es => es.Student)
			.Where(es => _context.Set<DocFile>()
				.Any(df => df.ExamStudentId == es.Id && df.ExamZipId == examZipId))
			.ToListAsync();
	}

	public async Task<List<ExamStudent>> GetByExamIdWithDetailsAsync(long examId, int skip, int take, ExamStudentStatus? statusFilter = null)
	{
		var query = _context.Set<ExamStudent>()
			.Include(es => es.Student)
			.Where(es => es.ExamId == examId);

		if (statusFilter.HasValue)
		{
			query = query.Where(es => es.Status == statusFilter.Value);
		}

		return await query
			.OrderBy(es => es.Student.StudentCode)
			.Skip(skip)
			.Take(take)
			.ToListAsync();
	}

	public async Task<int> CountByExamIdAsync(long examId, ExamStudentStatus? statusFilter = null)
	{
		var query = _context.Set<ExamStudent>().Where(es => es.ExamId == examId);

		if (statusFilter.HasValue)
		{
			query = query.Where(es => es.Status == statusFilter.Value);
		}

		return await query.CountAsync();
	}

		public async Task<List<ExamStudent>> GetExamStudentByExamId(int teacherId, long examId)
		{
			return await _context.ExamStudents
				.Include(es => es.Student)
				.Include(es => es.Teacher)
				.Include(es => es.Grades)
					.ThenInclude(g => g.Details)
						.ThenInclude(d => d.Rubric)
				.Where(es => es.Status != ExamStudentStatus.NOT_FOUND && es.TeacherId == teacherId && es.ExamId == examId
							 && es.Grades.Any(g => g.Status.Equals(GradeStatus.GRADED)))
				.ToListAsync();
		}

		public async Task<List<ExamStudent>> GetExamStudentByExamId(long examId)
		{
			return await _context.ExamStudents
				.Include(es => es.Student)
				.Include(es => es.Teacher)
				.Include(es => es.Grades)
					.ThenInclude(g => g.Details)
						.ThenInclude(d => d.Rubric)
				.Where(es => es.Status != ExamStudentStatus.NOT_FOUND && es.ExamId == examId
							 && es.Grades.Any(g => g.Status.Equals(GradeStatus.GRADED)))
				.ToListAsync();
		}


		public async Task<int> CountByExamIdAndTeacherIdAsync(long examId, int teacherId, ExamStudentStatus? statusFilter = null)
		{
			var query = _context.ExamStudents
		.Where(es => es.ExamId == examId && es.TeacherId == teacherId);

			if (statusFilter.HasValue)
				query = query.Where(es => es.Status == statusFilter.Value);

			return await query.CountAsync();
		}

		public async Task<List<ExamStudent>> GetByExamIdAndTeacherIdWithDetailsAsync(long examId, int teacherId, int skip, int take, ExamStudentStatus? statusFilter = null)
		{
			var query = _context.ExamStudents
	   .Include(es => es.Student)
	   .Include(es => es.Grades)
	   .Where(es => es.ExamId == examId && es.TeacherId == teacherId);

			if (statusFilter.HasValue)
				query = query.Where(es => es.Status == statusFilter.Value);

			return await query
				.OrderBy(es => es.Id)
				.Skip(skip)
				.Take(take)
				.ToListAsync();
		}
	}
}

