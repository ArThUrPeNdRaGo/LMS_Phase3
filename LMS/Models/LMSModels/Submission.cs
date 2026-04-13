using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public int SubmissionId { get; set; }
        public string StudentUId { get; set; } = null!;
        public int AssignmentId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public uint? Score { get; set; }
        public string Contents { get; set; } = null!;

        public virtual Assignment Assignment { get; set; } = null!;
        public virtual Student StudentU { get; set; } = null!;
    }
}
