
using AutoMapper;
using BLL.Model.Request.Grade;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Mapper
{
    public class GradeDetailProfile : Profile
    {
        public GradeDetailProfile()
        {
            CreateMap<GradeDetailRequest, GradeDetail>();
        }
    }
}
