using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false });
            }

            if (db.Departments.Any(d => d.SubjectAbbr == subject))
            {
                return Json(new { success = false });
            }

            var dept = new Department
            {
                SubjectAbbr = subject,
                DeptName = name
            };

            db.Departments.Add(dept);
            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subject">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var query = from c in db.Courses
                        where c.SubjectAbbr == subject
                        select new
                        {
                            number = c.CourseNum,
                            name = c.CourseName
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var query = from p in db.Professors
                        where p.WorksInDeptAbbr == subject
                        select new
                        {
                            lname = p.LastName,
                            fname = p.FirstName,
                            uid = p.UId
                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(name) || number <= 0)
            {
                return Json(new { success = false });
            }

            if (db.Courses.Any(c => c.SubjectAbbr == subject && c.CourseNum == (uint)number))
            {
                return Json(new { success = false });
            }

            var course = new Course
            {
                SubjectAbbr = subject,
                CourseNum = (uint)number,
                CourseName = name
            };

            db.Courses.Add(course);
            db.SaveChanges();

            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(season) || 
                string.IsNullOrWhiteSpace(location) || string.IsNullOrWhiteSpace(instructor) || 
                number <= 0 || year <= 0)
            {
                return Json(new { success = false });
            }

            var newStart = TimeOnly.FromDateTime(start);
            var newEnd = TimeOnly.FromDateTime(end);

            bool classExists = db.Classes.Any(c => 
                c.CourseSubjectAbbr == subject && 
                c.CourseNum == (uint)number && 
                c.SemesterSeason == season && 
                c.SemesterYear == (uint)year);

            if (classExists)
            {
                return Json(new { success = false });
            }

            bool locationConflict = db.Classes.Any(c =>
                c.SemesterYear == (uint)year &&
                c.SemesterSeason == season &&
                c.Location == location &&
                (newStart < c.EndTime && newEnd > c.StartTime));

            if (locationConflict)
            {
                return Json(new { success = false });
            }

            var newClass = new Class
            {
                SemesterYear = (uint)year,
                SemesterSeason = season,
                Location = location,
                StartTime = newStart,
                EndTime = newEnd,
                CourseSubjectAbbr = subject,
                CourseNum = (uint)number,
                ProfessorUId = instructor
            };

            db.Classes.Add(newClass);
            db.SaveChanges();

            return Json(new { success = true });
        }

        /*******End code to modify********/

    }
}