using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var cls = FindClass(subject, num, season, year);
            if (cls == null)
                return Json(Array.Empty<object>());

            var result =
                from e in db.Enrollments
                join s in db.Students on e.StudentUId equals s.UId
                where e.ClassId == cls.ClassId
                select new
                {
                    fname = s.FirstName,
                    lname = s.LastName,
                    uid = s.UId,
                    dob = s.Dob,
                    grade = e.Grade ?? "--"
                };

            return Json(result.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var cls = FindClass(subject, num, season, year);
            if (cls == null)
                return Json(Array.Empty<object>());

            var result =
                from cat in db.AssignmentCategories
                join a in db.Assignments on cat.CategoryId equals a.CategoryId
                where cat.ClassId == cls.ClassId &&
                      (category == null || cat.Name == category)
                select new
                {
                    aname = a.Name,
                    cname = cat.Name,
                    due = a.DueDatetime,
                    submissions = db.Submissions.Count(s => s.AssignmentId == a.AssignmentId)
                };

            return Json(result.ToArray());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var cls = FindClass(subject, num, season, year);
            if (cls == null)
                return Json(Array.Empty<object>());

            var result = db.AssignmentCategories
                .Where(c => c.ClassId == cls.ClassId)
                .Select(c => new
                {
                    name = c.Name,
                    weight = c.Weight
                });

            return Json(result.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var cls = FindClass(subject, num, season, year);
            if (cls == null)
                return Json(new { success = false });

            bool exists = db.AssignmentCategories.Any(c =>
                c.ClassId == cls.ClassId && c.Name == category);

            if (exists)
                return Json(new { success = false });

            AssignmentCategory newCat = new AssignmentCategory
            {
                ClassId = cls.ClassId,
                Name = category,
                Weight = (byte)catweight
            };

            db.AssignmentCategories.Add(newCat);
            db.SaveChanges();

            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var cls = FindClass(subject, num, season, year);
            if (cls == null)
                return Json(new { success = false });

            var cat = db.AssignmentCategories.FirstOrDefault(c =>
                c.ClassId == cls.ClassId && c.Name == category);

            if (cat == null)
                return Json(new { success = false });

            bool exists = db.Assignments.Any(a =>
                a.CategoryId == cat.CategoryId && a.Name == asgname);

            if (exists)
                return Json(new { success = false });

            Assignment newAssignment = new Assignment
            {
                CategoryId = cat.CategoryId,
                Name = asgname,
                MaxPoints = (uint)asgpoints,
                DueDatetime = asgdue,
                Contents = asgcontents
            };

            db.Assignments.Add(newAssignment);
            db.SaveChanges();

            // recalc grades
            var students = db.Enrollments
                .Where(e => e.ClassId == cls.ClassId)
                .Select(e => e.StudentUId)
                .ToList();

            foreach (var uid2 in students)
                RecalculateStudentGrade(cls.ClassId, uid2);

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var cls = FindClass(subject, num, season, year);
            if (cls == null)
                return Json(Array.Empty<object>());

            var assignment =
                (from cat in db.AssignmentCategories
                 join a in db.Assignments on cat.CategoryId equals a.CategoryId
                 where cat.ClassId == cls.ClassId &&
                       cat.Name == category &&
                       a.Name == asgname
                 select a).FirstOrDefault();

            if (assignment == null)
                return Json(Array.Empty<object>());

            var result =
                from sub in db.Submissions
                join s in db.Students on sub.StudentUId equals s.UId
                where sub.AssignmentId == assignment.AssignmentId
                orderby sub.SubmittedAt descending
                select new
                {
                    fname = s.FirstName,
                    lname = s.LastName,
                    uid = s.UId,
                    time = sub.SubmittedAt,
                    score = sub.Score
                };

            return Json(result.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var cls = FindClass(subject, num, season, year);
            if (cls == null)
                return Json(new { success = false });

            var assignment =
                (from cat in db.AssignmentCategories
                 join a in db.Assignments on cat.CategoryId equals a.CategoryId
                 where cat.ClassId == cls.ClassId &&
                       cat.Name == category &&
                       a.Name == asgname
                 select a).FirstOrDefault();

            if (assignment == null)
                return Json(new { success = false });

            var sub = db.Submissions
                .Where(s => s.AssignmentId == assignment.AssignmentId && s.StudentUId == uid)
                .OrderByDescending(s => s.SubmittedAt)
                .FirstOrDefault();

            if (sub == null)
                return Json(new { success = false });

            sub.Score = (uint)score;
            db.SaveChanges();

            // update grade
            RecalculateStudentGrade(cls.ClassId, uid);

            return Json(new { success = true });
        }

        /// <summary>
        /// Finds the class matching the given subject, number, season, and year. Returns null if not found.
        /// </summary>
        /// <param name="subject">The subject of class</param>
        /// <param name="num">The class number</param>
        /// <param name="season">The semester class takes place</param>
        /// <param name="year">The year the class takes places</param>
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
        /// Recalculates the grade for a student in a class based on their submissions and the assignment category weights, and updates the database with the new grade.
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="uid"></param>
        private void RecalculateStudentGrade(int classId, string uid)
        {
            var categories = db.AssignmentCategories
                .Where(c => c.ClassId == classId)
                .ToList();

            double weightedTotal = 0;
            double weightSum = 0;

            foreach (var cat in categories)
            {
                var assignments = db.Assignments
                    .Where(a => a.CategoryId == cat.CategoryId)
                    .ToList();

                if (assignments.Count == 0)
                    continue;

                double earned = 0;
                double possible = 0;

                foreach (var a in assignments)
                {
                    possible += a.MaxPoints;

                    var sub = db.Submissions
                        .Where(s => s.AssignmentId == a.AssignmentId && s.StudentUId == uid)
                        .OrderByDescending(s => s.SubmittedAt)
                        .FirstOrDefault();

                    earned += sub?.Score ?? 0;
                }

                if (possible == 0)
                    continue;

                double percent = earned / possible;
                weightedTotal += percent * cat.Weight;
                weightSum += cat.Weight;
            }

            var enrollment = db.Enrollments
                .FirstOrDefault(e => e.ClassId == classId && e.StudentUId == uid);

            if (enrollment == null)
                return;

            if (weightSum == 0)
            {
                enrollment.Grade = "--";
            }
            else
            {
                double finalPercent = weightedTotal * (100.0 / weightSum);
                enrollment.Grade = finalPercent >= 93 ? "A"
                    : finalPercent >= 90 ? "A-"
                    : finalPercent >= 87 ? "B+"
                    : finalPercent >= 83 ? "B"
                    : finalPercent >= 80 ? "B-"
                    : finalPercent >= 77 ? "C+"
                    : finalPercent >= 73 ? "C"
                    : finalPercent >= 70 ? "C-"
                    : finalPercent >= 67 ? "D+"
                    : finalPercent >= 63 ? "D"
                    : finalPercent >= 60 ? "D-"
                    : "E";
            }

            db.SaveChanges();
        }

        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var result =
                from c in db.Classes
                join co in db.Courses
                    on new { A = c.CourseSubjectAbbr, B = c.CourseNum }
                    equals new { A = co.SubjectAbbr, B = co.CourseNum }
                where c.ProfessorUId == uid
                select new
                {
                    subject = c.CourseSubjectAbbr,
                    number = c.CourseNum,
                    name = co.CourseName,
                    season = c.SemesterSeason,
                    year = c.SemesterYear
                };

            return Json(result.ToArray());
        }



        /*******End code to modify********/
    }
}

