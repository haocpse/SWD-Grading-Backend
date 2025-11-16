using AutoMapper;
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
		}

	}
}
