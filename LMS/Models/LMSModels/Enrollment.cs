using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Enrollment
    {
        public string StudentUId { get; set; } = null!;
        public int ClassId { get; set; }
        public string? Grade { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual Student StudentU { get; set; } = null!;
    }
}
