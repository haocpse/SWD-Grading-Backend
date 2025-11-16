using AutoMapper;
using BLL.Model.Request.Rubric;
using BLL.Model.Response.Rubric;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Mapper
{
	public class RubricProfile : Profile
	{
		public RubricProfile()
		{
			CreateMap<Rubric, RubricResponse>();
			CreateMap<UpdateRubricRequest, Rubric>();
			CreateMap<CreateRubricRequest, Rubric>();
		}

	}
}
