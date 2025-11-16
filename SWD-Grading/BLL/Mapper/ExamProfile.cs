using AutoMapper;
using BLL.Model.Request.Exam;
using BLL.Model.Response.Exam;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Mapper
{
	public class ExamProfile : Profile
	{

		public ExamProfile()
		{
			CreateMap<CreateExamRequest, Exam>();
			CreateMap<Exam, ExamResponse>();
			CreateMap<UpdateExamRequest, Exam>();
		}

	}
}
