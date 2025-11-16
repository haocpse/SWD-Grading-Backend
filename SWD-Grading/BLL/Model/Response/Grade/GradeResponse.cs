using Model.Entity;
using Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Response.Grade
{
    public class GradeResponse
    {
        public long Id { get; set; }
        public long ExamStudentId { get; set; }
        public decimal TotalScore { get; set; }

        public string? Comment { get; set; }

        public DateTime? GradedAt { get; set; }
        public string? GradedBy { get; set; }
        public string Status { get; set; }
    }
}
