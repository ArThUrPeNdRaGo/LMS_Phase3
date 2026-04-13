using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            Enrollments = new HashSet<Enrollment>();
        }

        public int ClassId { get; set; }
        public uint SemesterYear { get; set; }
        public string SemesterSeason { get; set; } = null!;
        public string Location { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string CourseSubjectAbbr { get; set; } = null!;
        public uint CourseNum { get; set; }
        public string ProfessorUId { get; set; } = null!;

        public virtual Course Course { get; set; } = null!;
        public virtual Professor ProfessorU { get; set; } = null!;
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
