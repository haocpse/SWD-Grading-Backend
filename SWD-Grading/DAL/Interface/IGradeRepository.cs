using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interface
{
    public interface IGradeRepository : IGenericRepository<Grade, long>
    {
        Task<IEnumerable<Grade>> GetAll();
        Task<Grade?> GetById(long id);
        Task<IEnumerable<Grade>> GetByExamStudentId(long examStudentId);
    }
}
