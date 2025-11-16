using Amazon.S3.Model.Internal.MarshallTransformations;
using AutoMapper;
using BLL.Interface;
using BLL.Model.Request;
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
    }
}
