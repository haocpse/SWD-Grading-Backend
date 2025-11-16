using AutoMapper;
using BLL.Exceptions;
using BLL.Interface;
using BLL.Model.Request.Rubric;
using BLL.Model.Response.Rubric;
using DAL.Interface;
using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
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
			var question = await _unitOfWork.ExamQuestionRepository.GetByIdAsync(questionId);
			if (question == null)
				throw new AppException("Question not found", 404);

			// Get all rubrics of this question
			var existing = await _unitOfWork.RubricRepository.GetRubricByQuestionId(questionId);

			decimal newTotal = existing.Sum(r => r.MaxScore) + request.MaxScore;

			if (newTotal > question.MaxScore)
				throw new AppException($"Total rubric scores ({newTotal}) exceeds question max score {question.MaxScore}", 400);

			if (newTotal < question.MaxScore)
				throw new AppException($"Total rubric scores ({newTotal}) must equal question max score {question.MaxScore}", 400);

			// Create entity
			Rubric rubric = _mapper.Map<Rubric>(request);
			rubric.ExamQuestionId = questionId;

			await _unitOfWork.RubricRepository.AddAsync(rubric);

			// Add to list for indexing
			existing.ToList().Add(rubric);

			// Reorder OrderIndex
			ReorderRubrics(existing.ToList());

			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<RubricResponse>(rubric);
		}

		public async Task DeleteAsync(long id)
		{
			var rubric = await _unitOfWork.RubricRepository.GetByIdAsync(id);
			if (rubric == null)
				throw new AppException("Rubric not found", 404);

			long questionId = rubric.ExamQuestionId;

			// Xóa rubric
			await _unitOfWork.RubricRepository.RemoveAsync(rubric);

			// Lấy lại danh sách rubric còn lại của câu hỏi
			var rubrics = await _unitOfWork.RubricRepository.GetRubricByQuestionId(questionId);

			// Reorder OrderIndex
			ReorderRubrics(rubrics.ToList());

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

			var question = await _unitOfWork.ExamQuestionRepository.GetByIdAsync(rubric.ExamQuestionId);
			if (question == null)
				throw new AppException("Question not found", 404);

			// Get all rubrics in question
			var rubrics = await _unitOfWork.RubricRepository.GetRubricByQuestionId(question.Id);

			// Calculate new total after update
			decimal oldTotal = rubrics.Sum(r => r.Id == id ? request.MaxScore : r.MaxScore);

			if (oldTotal > question.MaxScore)
				throw new AppException($"Total rubric scores ({oldTotal}) exceeds question max score {question.MaxScore}", 400);

			if (oldTotal < question.MaxScore)
				throw new AppException($"Total rubric scores ({oldTotal}) must equal question max score {question.MaxScore}", 400);

			// Apply updates
			_mapper.Map(request, rubric);

			await _unitOfWork.RubricRepository.UpdateAsync(rubric);

			// Reorder order index
			ReorderRubrics(rubrics.ToList());

			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<RubricResponse>(rubric);
		}

		private void ReorderRubrics(List<Rubric> rubrics)
		{
			int index = 1;
			foreach (var r in rubrics.OrderBy(x => x.OrderIndex).ThenBy(x => x.Id))
			{
				r.OrderIndex = index++;
			}
		}

	}
}
