using AutoMapper;
using BLL.Model.Request.Student;
using BLL.Model.Response.Student;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Mapper
{
	public class StudentProfile : Profile
	{
		public StudentProfile()
		{
			CreateMap<Student, StudentResponse>();
			CreateMap<CreateStudentRequest, Student>();
			CreateMap<UpdateStudentRequest, Student>();
		}
	}
}
