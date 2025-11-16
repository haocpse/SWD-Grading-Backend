using BLL.Model.Request;
using BLL.Model.Response;
using BLL.Model.Response.Grade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public interface IGradeService
    {
        Task<PagingResponse<GradeResponse>> GetAll(PagedRequest request);
    }
}
