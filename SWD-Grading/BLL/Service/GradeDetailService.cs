using AutoMapper;
using BLL.Interface;
using BLL.Model.Request.Grade;
using DAL.Interface;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class GradeDetailService : IGradeDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GradeDetailService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task Create(GradeDetailRequest request)
        {
            var gradeDetailEntity = _mapper.Map<GradeDetail>(request);
            await _unitOfWork.GradeDetailRepository.AddAsync(gradeDetailEntity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task Update(GradeDetailRequest request, long id)
        {
            var existingGradeDetail = await _unitOfWork.GradeDetailRepository.GetByIdAsync(id);
            if (existingGradeDetail != null)
            {
                _mapper.Map(request, existingGradeDetail);
                await _unitOfWork.GradeDetailRepository.UpdateAsync(existingGradeDetail);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                throw new Exception("GradeDetail not found");
            }
        }
    }
}
