using AutoMapper;
using BLL.Exceptions;
using BLL.Interface;
using BLL.Model.Request.Exam;
using BLL.Model.Response;
using BLL.Model.Response.Exam;
using DAL.Interface;
using Model.Entity;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
	public class ExamService : IExamService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		public ExamService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<ExamResponse> CreateExam(CreateExamRequest request)
		{
			Exam exam = _mapper.Map<Exam>(request);
			await _unitOfWork.ExamRepository.AddAsync(exam);
			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<ExamResponse>(exam); 
		}

		public async Task<bool> DeleteAsync(long id)
		{
			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(id);
			if (exam == null) 
				throw new AppException("Exam not found", 404);
			await _unitOfWork.ExamRepository.RemoveAsync(exam);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		public async Task<PagingResponse<ExamResponse>> GetAllAsync(ExamFilter filter)
		{
			var filters = new List<Expression<Func<Exam, bool>>>();
			if (filter.Page <= 0)
				throw new AppException("Page number must be greater than or equal to 1", 400);
			if (filter.Size < 0)
				throw new AppException("Size must not be negative", 400);
			var skip = (filter.Page - 1) * filter.Size;
			var totalItems = await _unitOfWork.ExamRepository.CountAsync(filters);
			Func<IQueryable<Exam>, IOrderedQueryable<Exam>> orderBy = q => q.OrderByDescending(o => o.CreatedAt);
			var data = await _unitOfWork.ExamRepository.GetPagedAsync<Exam>(
				skip, filter.Size, filters, orderBy, null, null, asNoTracking: true);
			var respones = _mapper.Map<IEnumerable<ExamResponse>>(data.ToList());
			return new()
			{
				Result = respones,
				Page = filter.Page,
				Size = filter.Size,
				TotalItems = totalItems,
				TotalPages = (int)Math.Ceiling((double)totalItems / filter.Size)
			};
		}

		public async Task<ExamResponse?> GetByIdAsync(long id)
		{
			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(id);
			return exam == null ? 
				throw new AppException("Exam not found", 404) : 
				_mapper.Map<ExamResponse>(exam);
		}

		public async Task<ExamResponse?> UpdateAsync(long id, UpdateExamRequest request)
		{
			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(id);
			if (exam == null)
				throw new AppException("Exam not found", 404);
			_mapper.Map(request, exam);
			await _unitOfWork.ExamRepository.UpdateAsync(exam);
			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<ExamResponse>(exam);
		}
	}
}
