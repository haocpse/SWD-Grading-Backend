using BLL.Model.Request.Grade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public interface IGradeDetailService
    {
        Task Create(GradeDetailRequest request);
        Task Update(GradeDetailRequest request, long id);
    }
}
