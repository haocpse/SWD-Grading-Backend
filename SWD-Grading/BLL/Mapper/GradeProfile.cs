using AutoMapper;
using BLL.Model.Request.Grade;
using BLL.Model.Response.Grade;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Mapper
{
    public class GradeProfile : Profile
    {
        public GradeProfile()
        {
            CreateMap<Grade, GradeResponse>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Grade, GradeDetailResponse>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Details,
                    opt => opt.MapFrom(src => src.Details));

            // Map GradeDetail entity to GradeDetailModel
            CreateMap<GradeDetail, GradeDetailModel>();
            CreateMap<GradeRequest, Grade>();
        }
    }
}
