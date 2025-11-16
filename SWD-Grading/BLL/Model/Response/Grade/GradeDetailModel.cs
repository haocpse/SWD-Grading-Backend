using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Response.Grade
{
    public class GradeDetailModel
    {
        public long Id { get; set; }
        public long RubricId { get; set; }
        public decimal Score { get; set; }
        public string? Comment { get; set; } 
        public string? AutoDetectResult { get; set; }
    }
}
