using BLL.Model.Request.Exam;
using BLL.Model.Response;
using BLL.Model.Response.Exam;
using BLL.Model.Response.Grade;
using Microsoft.AspNetCore.Http;
using Model.Entity;
using Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public interface IExamService
    {
        Task<ExamResponse> CreateExam(CreateExamRequest request);
        Task<ExamResponse?> UpdateAsync(long id, UpdateExamRequest request);
        Task<PagingResponse<ExamResponse>> GetAllAsync(ExamFilter filter);
        Task<ExamResponse?> GetByIdAsync(long id);
        Task<bool> DeleteAsync(long id);
        Task ParseDetailExcel(long id, IFormFile file);
        Task<ExamResponse?> GetQuestionByExamId(long id);
        Task<GradeExportResponse> ExportGradeExcel(int userId, UserRole role, long id);
		Task<PagingResponse<ExamResponse>> GetAssignedExam(ExamFilter filter, int userId);
        Task<List<GradeExportResponse>> GetGradeHistory(long id);
		Task<List<GradeExportResponse>> GetMyGradeHistory(int teacherId, long id);
	}
}
