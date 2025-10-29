using Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
	public class User
	{
		public int Id { get; set; }
		public string Username { get; set; }
		public string PasswordHash { get; set; }
		public bool IsActive { get; set; }
		public UserRole Role { get; set; }
	}
}
