using Amazon.S3.Model.Internal.MarshallTransformations;
using AutoMapper;
using BLL.Interface;
using BLL.Model.Request;
using BLL.Model.Request.Grade;
using BLL.Model.Response;
using BLL.Model.Response.Grade;
using DAL.Interface;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class GradeService : IGradeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GradeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagingResponse<GradeResponse>> GetAllByExamStudentId(long examStudentId, PagedRequest request)
        {
            var query = _unitOfWork.GradeRepository
                .Query(asNoTracking: true)
                .Where(g => g.ExamStudentId == examStudentId);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);

            var gradeEntities = await query
                .OrderByDescending(g => g.GradedAt)
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync();

            var grades = _mapper.Map<List<GradeResponse>>(gradeEntities);

            var pagedResponse = new PagingResponse<GradeResponse>
            {
                Page = request.PageIndex,
                Size = request.PageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                Result = grades
            };

            return pagedResponse;
        }

        public async Task<PagingResponse<GradeResponse>> GetAll(PagedRequest request)
        {
            var query = _unitOfWork.GradeRepository.Query(asNoTracking: true);
            var totalItems = await _unitOfWork.GradeRepository.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);
            var gradeEntities = await query
                .OrderByDescending(g => g.GradedAt)
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync();

            // Then map in memory (not in SQL)
            var grades = _mapper.Map<List<GradeResponse>>(gradeEntities);

            var pagedResponse = new PagingResponse<GradeResponse>
            {
                Page = request.PageIndex,
                Size = request.PageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                Result = grades
            };

            return pagedResponse;
        }

        public async Task<GradeDetailResponse> GetById(long id)
        {
            var grade = await _unitOfWork.GradeRepository.GetById(id);
            var gradeDetailResponse = _mapper.Map<GradeDetailResponse>(grade);
            return gradeDetailResponse;
        }

        public async Task<long> Create(GradeRequest request)
        {
            var gradeEntity = _mapper.Map<Grade>(request);
            await _unitOfWork.GradeRepository.AddAsync(gradeEntity);
            await _unitOfWork.SaveChangesAsync();
            return gradeEntity.Id;
        }

        public async Task Update(GradeRequest request, long id)
        {
            var existingGrade = await _unitOfWork.GradeRepository.GetById(id);
            if (existingGrade == null)
            {
                throw new KeyNotFoundException("Grade not found");
            }
            _mapper.Map(request, existingGrade);
            await _unitOfWork.GradeRepository.UpdateAsync(existingGrade);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task Delete(long id)
        {
            var existingGrade = await _unitOfWork.GradeRepository.GetById(id);
            var existingDetails = await _unitOfWork.GradeDetailRepository.GetByGradeId(id);
            if (existingGrade == null)
            {
                throw new KeyNotFoundException("Grade not found");
            }
            foreach (var detail in existingDetails)
            {
                await _unitOfWork.GradeDetailRepository.RemoveAsync(detail);
            }
            await _unitOfWork.GradeRepository.RemoveAsync(existingGrade);
            await _unitOfWork.SaveChangesAsync();
        }
    }

}
