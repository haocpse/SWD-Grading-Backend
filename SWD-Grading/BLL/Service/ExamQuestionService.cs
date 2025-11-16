using AutoMapper;
using BLL.Exceptions;
using BLL.Interface;
using BLL.Model.Request.ExamQuestion;
using BLL.Model.Response.ExamQuestion;
using DAL.Interface;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
	public class ExamQuestionService : IExamQuestionService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		public ExamQuestionService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}


		public async Task<bool> DeleteAsync(long id)
		{
			var x = await _unitOfWork.ExamQuestionRepository.GetByIdAsync(id);
			if (x == null)
				throw new AppException("Question not found", 404);
			await _unitOfWork.ExamQuestionRepository.RemoveAsync(x);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		public async Task<ExamQuestionResponse?> GetByIdAsync(long id)
		{
			var x = await _unitOfWork.ExamQuestionRepository.GetByIdAsync(id);
			if (x == null)
				throw new AppException("Question not found", 404);

			return _mapper.Map<ExamQuestionResponse>(x);

		}

		public async Task<ExamQuestionResponse?> UpdateAsync(long id, UpdateExamQuestionRequest request)
		{
			var x = await _unitOfWork.ExamQuestionRepository.GetByIdAsync(id);
			if (x == null)
				throw new AppException("Question not found", 404);

			_mapper.Map(request, x);
			await _unitOfWork.ExamQuestionRepository.UpdateAsync(x);
			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<ExamQuestionResponse>(x);
		}
	}
}
