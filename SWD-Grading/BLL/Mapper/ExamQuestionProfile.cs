using AutoMapper;
using BLL.Model.Response.ExamQuestion;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Mapper
{
	public class ExamQuestionProfile : Profile
	{

		public ExamQuestionProfile()
		{
			CreateMap<ExamQuestion, ExamQuestionResponse>();	
		}

	}
}
