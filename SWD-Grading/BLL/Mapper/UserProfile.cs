using AutoMapper;
using BLL.Model.Request.Auth;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Mapper
{
	public class UserProfile : Profile
	{
		public UserProfile() {
            CreateMap<RegisterRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        }

	}
}
