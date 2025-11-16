using AutoMapper;
using BLL.Exceptions;
using BLL.Interface;
using BLL.Model.Request.Student;
using BLL.Model.Response;
using BLL.Model.Response.Student;
using DAL.Interface;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
	public class StudentService : IStudentService
	{
		private readonly IUnitOfWork _uow;
		private readonly IMapper _mapper;

		public StudentService(IUnitOfWork uow, IMapper mapper)
		{
			_uow = uow;
			_mapper = mapper;
		}
		public async Task<StudentResponse> CreateAsync(CreateStudentRequest request)
		{
			var entity = _mapper.Map<Student>(request);
			var exists = await _uow.StudentRepository
					.ExistsByStudentCodeAsync(request.StudentCode);

			if (exists)
				throw new AppException("StudentCode already exists", 400);
			await _uow.StudentRepository.AddAsync(entity);
			await _uow.SaveChangesAsync();

			return _mapper.Map<StudentResponse>(entity);
		}

		public async Task<bool> DeleteAsync(long id)
		{
			var entity = await _uow.StudentRepository.GetByIdAsync(id);
			if (entity == null)
				throw new AppException("Student not found", 404);
			await _uow.StudentRepository.RemoveAsync(entity);
			await _uow.SaveChangesAsync();
			return true;
		}

		public async Task<PagingResponse<StudentResponse>> GetAllAsync(StudentFilter filter)
		{
			if (filter.Page <= 0)
				throw new AppException("Page number must be greater than or equal to 1", 400);

			if (filter.Size < 0)
				throw new AppException("Size must not be negative", 400);

			var filters = new List<Expression<Func<Student, bool>>>();

			var skip = (filter.Page - 1) * filter.Size;

			var totalItems = await _uow.StudentRepository.CountAsync(filters);

			Func<IQueryable<Student>, IOrderedQueryable<Student>> orderBy =
				q => q.OrderBy(x => x.Id);

			var data = await _uow.StudentRepository.GetPagedAsync<Student>(
				skip,
				filter.Size,
				filters,
				orderBy,
				include: null,
				null,
				asNoTracking: true
			);

			var responses = _mapper.Map<IEnumerable<StudentResponse>>(data);

			return new PagingResponse<StudentResponse>
			{
				Result = responses,
				Page = filter.Page,
				Size = filter.Size,
				TotalItems = totalItems,
				TotalPages = (int)Math.Ceiling((double)totalItems / filter.Size)
			};
		}

		public async Task<StudentResponse?> GetByIdAsync(long id)
		{
			var entity = await _uow.StudentRepository.GetByIdAsync(id);
			if (entity == null)
				throw new AppException("Student not found", 404);
			return _mapper.Map<StudentResponse>(entity);
		}

		public async Task<StudentResponse?> UpdateAsync(long id, UpdateStudentRequest request)
		{
			var entity = await _uow.StudentRepository.GetByIdAsync(id);
			if (entity == null)
				throw new AppException("Student not found", 404);
			var exists = await _uow.StudentRepository
				.ExistsByStudentCodeAsync(request.StudentCode, excludeId: id);

			if (exists)
				throw new AppException("StudentCode already exists", 400);
			_mapper.Map(request, entity);

			await _uow.StudentRepository.UpdateAsync(entity);
			await _uow.SaveChangesAsync();

			return _mapper.Map<StudentResponse>(entity);
		}
	}
}
