using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LMS.Models.LMSModels
{
    public partial class LMSContext : DbContext
    {
        public LMSContext()
        {
        }

        public LMSContext(DbContextOptions<LMSContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Administrator> Administrators { get; set; } = null!;
        public virtual DbSet<Assignment> Assignments { get; set; } = null!;
        public virtual DbSet<AssignmentCategory> AssignmentCategories { get; set; } = null!;
        public virtual DbSet<Class> Classes { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<Enrollment> Enrollments { get; set; } = null!;
        public virtual DbSet<Professor> Professors { get; set; } = null!;
        public virtual DbSet<Sshkey> Sshkeys { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<Submission> Submissions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("name=LMS:LMSConnectionString", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.11.16-mariadb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");

            modelBuilder.Entity<Administrator>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.Dob).HasColumnName("dob");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .HasColumnName("last_name");
            });

            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasIndex(e => new { e.CategoryId, e.Name }, "category_id")
                    .IsUnique();

                entity.Property(e => e.AssignmentId)
                    .HasColumnType("int(11)")
                    .HasColumnName("assignment_id");

                entity.Property(e => e.CategoryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("category_id");

                entity.Property(e => e.Contents)
                    .HasColumnType("text")
                    .HasColumnName("contents");

                entity.Property(e => e.DueDatetime)
                    .HasColumnType("datetime")
                    .HasColumnName("due_datetime");

                entity.Property(e => e.MaxPoints)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("max_points");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Assignments)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Assignments_ibfk_1");
            });

            modelBuilder.Entity<AssignmentCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => new { e.ClassId, e.Name }, "class_id")
                    .IsUnique();

                entity.Property(e => e.CategoryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("category_id");

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("class_id");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.Weight)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("weight");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.AssignmentCategories)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("AssignmentCategories_ibfk_1");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasIndex(e => new { e.CourseSubjectAbbr, e.CourseNum }, "course_subject_abbr");

                entity.HasIndex(e => e.ProfessorUId, "professor_uID");

                entity.HasIndex(e => new { e.SemesterYear, e.SemesterSeason, e.CourseSubjectAbbr, e.CourseNum }, "semester_year")
                    .IsUnique();

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("class_id");

                entity.Property(e => e.CourseNum)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("course_num");

                entity.Property(e => e.CourseSubjectAbbr)
                    .HasMaxLength(4)
                    .HasColumnName("course_subject_abbr");

                entity.Property(e => e.EndTime)
                    .HasColumnType("time")
                    .HasColumnName("end_time");

                entity.Property(e => e.Location)
                    .HasMaxLength(100)
                    .HasColumnName("location");

                entity.Property(e => e.ProfessorUId)
                    .HasMaxLength(8)
                    .HasColumnName("professor_uID")
                    .IsFixedLength();

                entity.Property(e => e.SemesterSeason)
                    .HasColumnType("enum('Spring','Summer','Fall')")
                    .HasColumnName("semester_season");

                entity.Property(e => e.SemesterYear)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("semester_year");

                entity.Property(e => e.StartTime)
                    .HasColumnType("time")
                    .HasColumnName("start_time");

                entity.HasOne(d => d.ProfessorU)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.ProfessorUId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Classes_ibfk_2");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => new { d.CourseSubjectAbbr, d.CourseNum })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Classes_ibfk_1");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => new { e.SubjectAbbr, e.CourseNum })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.Property(e => e.SubjectAbbr)
                    .HasMaxLength(4)
                    .HasColumnName("subject_abbr");

                entity.Property(e => e.CourseNum)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("course_num");

                entity.Property(e => e.CourseName)
                    .HasMaxLength(100)
                    .HasColumnName("course_name");

                entity.HasOne(d => d.SubjectAbbrNavigation)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.SubjectAbbr)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Courses_ibfk_1");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.SubjectAbbr)
                    .HasName("PRIMARY");

                entity.Property(e => e.SubjectAbbr)
                    .HasMaxLength(4)
                    .HasColumnName("subject_abbr");

                entity.Property(e => e.DeptName)
                    .HasMaxLength(100)
                    .HasColumnName("dept_name");
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => new { e.StudentUId, e.ClassId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasIndex(e => e.ClassId, "class_id");

                entity.Property(e => e.StudentUId)
                    .HasMaxLength(8)
                    .HasColumnName("student_uID")
                    .IsFixedLength();

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("class_id");

                entity.Property(e => e.Grade)
                    .HasMaxLength(2)
                    .HasColumnName("grade")
                    .IsFixedLength();

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrollments_ibfk_2");

                entity.HasOne(d => d.StudentU)
                    .WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.StudentUId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrollments_ibfk_1");
            });

            modelBuilder.Entity<Professor>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.WorksInDeptAbbr, "works_in_dept_abbr");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.Dob).HasColumnName("dob");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .HasColumnName("last_name");

                entity.Property(e => e.WorksInDeptAbbr)
                    .HasMaxLength(4)
                    .HasColumnName("works_in_dept_abbr");

                entity.HasOne(d => d.WorksInDeptAbbrNavigation)
                    .WithMany(p => p.Professors)
                    .HasForeignKey(d => d.WorksInDeptAbbr)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Professors_ibfk_1");
            });

            modelBuilder.Entity<Sshkey>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("sshkey");

                entity.Property(e => e.Sshkey1)
                    .HasColumnType("text")
                    .HasColumnName("sshkey");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.MajorDeptAbbr, "major_dept_abbr");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.Dob).HasColumnName("dob");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .HasColumnName("last_name");

                entity.Property(e => e.MajorDeptAbbr)
                    .HasMaxLength(4)
                    .HasColumnName("major_dept_abbr");

                entity.HasOne(d => d.MajorDeptAbbrNavigation)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.MajorDeptAbbr)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Students_ibfk_1");
            });

            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasIndex(e => e.AssignmentId, "assignment_id");

                entity.HasIndex(e => new { e.StudentUId, e.AssignmentId, e.SubmittedAt }, "student_uID")
                    .IsUnique();

                entity.Property(e => e.SubmissionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("submission_id");

                entity.Property(e => e.AssignmentId)
                    .HasColumnType("int(11)")
                    .HasColumnName("assignment_id");

                entity.Property(e => e.Contents)
                    .HasColumnType("text")
                    .HasColumnName("contents");

                entity.Property(e => e.Score)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("score");

                entity.Property(e => e.StudentUId)
                    .HasMaxLength(8)
                    .HasColumnName("student_uID")
                    .IsFixedLength();

                entity.Property(e => e.SubmittedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("submitted_at");

                entity.HasOne(d => d.Assignment)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.AssignmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submissions_ibfk_2");

                entity.HasOne(d => d.StudentU)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.StudentUId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submissions_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
