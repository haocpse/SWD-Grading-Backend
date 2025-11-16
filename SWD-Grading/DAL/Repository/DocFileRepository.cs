using DAL.Interface;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
	public class DocFileRepository : GenericRepository<DocFile, long>, IDocFileRepository
	{
		private readonly SWDGradingDbContext _context;

		public DocFileRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task BulkInsertAsync(List<DocFile> docFiles)
		{
			await _context.Set<DocFile>().AddRangeAsync(docFiles);
		}

	public async Task<List<DocFile>> GetByExamStudentIdAsync(long examStudentId)
	{
		return await _context.Set<DocFile>()
			.Where(df => df.ExamStudentId == examStudentId)
			.ToListAsync();
	}

	public async Task<List<DocFile>> GetByExamStudentIdsAsync(List<long> examStudentIds)
	{
		return await _context.Set<DocFile>()
			.Where(df => examStudentIds.Contains(df.ExamStudentId))
			.ToListAsync();
	}

	public async Task<List<DocFile>> GetRecentlyParsedDocFilesAsync(int limit = 10)
	{
		return await _context.Set<DocFile>()
			.Include(df => df.ExamStudent)
			.ThenInclude(es => es.Student)
			.Where(df => df.ParseStatus == Model.Enums.DocParseStatus.OK 
				&& !string.IsNullOrWhiteSpace(df.ParsedText)
				&& !df.IsEmbedded) // Only get documents that haven't been embedded yet
			.OrderByDescending(df => df.Id)
			.Take(limit)
			.ToListAsync();
	}
}
}

