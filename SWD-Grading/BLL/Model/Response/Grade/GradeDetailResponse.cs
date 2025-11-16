using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Response.Grade
{
    public class GradeDetailResponse
    {
        public long ExamStudentId { get; set; }
        public decimal TotalScore { get; set; }

        public string? Comment { get; set; }

        public DateTime? GradedAt { get; set; }
        public string? GradedBy { get; set; }
        public string Status { get; set; }
        public List<GradeDetailModel> Details { get; set; }
    }
}
