using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Response
{
    public class LoginResponse
    {
        public int UserId { get; set; }
        public string username { get; set; }
        public string token { get; set; }
        public string role { get; set; }
    }
}
