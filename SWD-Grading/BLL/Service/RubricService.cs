using AutoMapper;
using BLL.Exceptions;
using BLL.Interface;
using BLL.Model.Request.Rubric;
using BLL.Model.Response.Rubric;
using DAL.Interface;
using DocumentFormat.OpenXml.Office2010.Excel;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
	public class RubricService : IRubricService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		public RubricService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<RubricResponse> CreateAsync(long questionId, CreateRubricRequest request)
		{
			var question = _unitOfWork.ExamQuestionRepository.GetByIdAsync(questionId);
			if (question == null)
				throw new AppException("Question not found", 404);
			Rubric rubric = _mapper.Map<Rubric>(request);
			rubric.ExamQuestionId = questionId;
			await _unitOfWork.RubricRepository.AddAsync(rubric);
			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<RubricResponse>(rubric);
		}

		public async Task DeleteAsync(long id)
		{
			var rubric = await _unitOfWork.RubricRepository.GetByIdAsync(id);
			if (rubric == null)
				throw new AppException("Rubric not found", 404);
			await _unitOfWork.RubricRepository.RemoveAsync(rubric);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<IEnumerable<RubricResponse>> GetRubricByQuestionId(long id)
		{
			var items = await _unitOfWork.RubricRepository.GetRubricByQuestionId(id);
			return _mapper.Map<List<RubricResponse>>(items.ToList());
		}

		public async Task<RubricResponse> UpdateAsync(long id, UpdateRubricRequest request)
		{
			var rubric = await _unitOfWork.RubricRepository.GetByIdAsync(id);
			if (rubric == null)
				throw new AppException("Rubric not found", 404);
			_mapper.Map(request, rubric);
			await _unitOfWork.RubricRepository.UpdateAsync(rubric);
			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<RubricResponse>(rubric);
		}
	}
}
