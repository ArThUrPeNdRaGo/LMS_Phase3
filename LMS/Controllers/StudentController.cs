using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the student is enrolled in.
        /// Each object in the array has:
        /// "subject", "number", "name", "season", "year", "grade"
        /// </summary>
        /// <param name="uid">The student's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var result =
                from e in db.Enrollments
                join c in db.Classes on e.ClassId equals c.ClassId
                join co in db.Courses
                    on new { A = c.CourseSubjectAbbr, B = c.CourseNum }
                    equals new { A = co.SubjectAbbr, B = co.CourseNum }
                where e.StudentUId == uid
                select new
                {
                    subject = c.CourseSubjectAbbr,
                    number = c.CourseNum,
                    name = co.CourseName,
                    season = c.SemesterSeason,
                    year = c.SemesterYear,
                    grade = e.Grade ?? "--"
                };

            return Json(result.ToArray());
        }

        /// <summary>
        /// Gets a JSON array of all the assignments in a class along with the student's submission (if any) and score for each assignment.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="num"></param>
        /// <param name="season"></param>
        /// <param name="year"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var cls = FindClass(subject, num, season, year);
            if (cls == null)
                return Json(Array.Empty<object>());

            var query1 =
                from cat in db.AssignmentCategories
                join a in db.Assignments on cat.CategoryId equals a.CategoryId
                where cat.ClassId == cls.ClassId
                select new
                {
                    AssignmentId = a.AssignmentId,
                    AssignmentName = a.Name,
                    CategoryName = cat.Name,
                    Due = a.DueDatetime
                };

            var result =
                from q in query1
                join s in db.Submissions
                on new { A = q.AssignmentId, B = uid }
                equals new { A = s.AssignmentId, B = s.StudentUId }
                into joined
                from j in joined.DefaultIfEmpty()
                select new
                {
                    aname = q.AssignmentName,
                    cname = q.CategoryName,
                    due = q.Due,
                    score = j == null ? null : j.Score
                };

            return Json(result.ToArray());
        }

        /// <summary>
        /// Submits the given text as a submission for the given assignment for the given student.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="num"></param>
        /// <param name="season"></param>
        /// <param name="year"></param>
        /// <param name="category"></param>
        /// <param name="asgname"></param>
        /// <param name="uid"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public IActionResult SubmitAssignmentText(
            string subject,
            int num,
            string season,
            int year,
            string category,
            string asgname,
            string uid,
            string contents)
        {
            var cls = FindClass(subject, num, season, year);
            if (cls == null)
                return Json(new { success = false });

            var assignment =
                (from cat in db.AssignmentCategories
                 join a in db.Assignments on cat.CategoryId equals a.CategoryId
                 where cat.ClassId == cls.ClassId
                       && cat.Name == category
                       && a.Name == asgname
                 select a).FirstOrDefault();

            if (assignment == null)
                return Json(new { success = false });

            var existing = db.Submissions.FirstOrDefault(s =>
                s.StudentUId == uid && s.AssignmentId == assignment.AssignmentId);

            if (existing == null)
            {
                Submission sub = new Submission
                {
                    StudentUId = uid,
                    AssignmentId = assignment.AssignmentId,
                    SubmittedAt = DateTime.Now,
                    Score = 0,
                    Contents = contents
                };

                db.Submissions.Add(sub);
            }
            else
            {
                existing.Contents = contents;
                existing.SubmittedAt = DateTime.Now;
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        /// <summary>
        /// Enrolls the given student in the given class. Returns success = false if the class doesn't exist or the student is already enrolled.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="num"></param>
        /// <param name="season"></param>
        /// <param name="year"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var cls = FindClass(subject, num, season, year);
            if (cls == null)
                return Json(new { success = false });

            bool alreadyEnrolled = db.Enrollments.Any(e =>
                e.StudentUId == uid && e.ClassId == cls.ClassId);

            if (alreadyEnrolled)
                return Json(new { success = false });

            Enrollment enrollment = new Enrollment
            {
                StudentUId = uid,
                ClassId = cls.ClassId,
                Grade = "--"
            };

            db.Enrollments.Add(enrollment);
            db.SaveChanges();

            return Json(new { success = true });
        }

        /// <summary>
        /// Calculates and returns the GPA for the given student based on their enrollments.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public IActionResult GetGPA(string uid)
        {
            var grades = db.Enrollments
                .Where(e => e.StudentUId == uid && e.Grade != null && e.Grade != "--")
                .Select(e => e.Grade!)
                .ToList();

            if (grades.Count == 0)
                return Json(new { gpa = 0.0 });

            var points = grades
                .Select(g => GradeToPoints(g))
                .Where(p => p >= 0.0)
                .ToList();

            if (points.Count == 0)
                return Json(new { gpa = 0.0 });

            double gpa = points.Average();
            return Json(new { gpa = gpa });
        }

        /// <summary>
        /// Finds a class by subject/number/season/year.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="num"></param>
        /// <param name="season"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private Class? FindClass(string subject, int num, string season, int year)
        {
            return db.Classes.FirstOrDefault(c =>
                c.CourseSubjectAbbr == subject &&
                c.CourseNum == (uint)num &&
                c.SemesterSeason == season &&
                c.SemesterYear == (uint)year);
        }

        /// <summary>
        /// Converts a letter grade to GPA points.
        /// </summary>
        /// <param name="grade"></param>
        /// <returns></returns>
        private double GradeToPoints(string grade)
        {
            return grade switch
            {
                "A" => 4.0,
                "A-" => 3.7,
                "B+" => 3.3,
                "B" => 3.0,
                "B-" => 2.7,
                "C+" => 2.3,
                "C" => 2.0,
                "C-" => 1.7,
                "D+" => 1.3,
                "D" => 1.0,
                "D-" => 0.7,
                "E" => 0.0,
                _ => -1.0
            };
        }

        /*******End code to modify********/

    }
}

