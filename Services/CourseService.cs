using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CourseResponse> CreateCourseAsync(CreateCourseDto dto)
        {
            var existing = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == dto.CourseCode);
            if (existing != null)
                throw new Exception($"Course with code {dto.CourseCode} already exists");

            var course = new Course
            {
                CourseCode = dto.CourseCode,
                CourseName = dto.CourseName,
                Credits = dto.Credits,
                Department = dto.Department,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return MapToResponse(course);
        }

        public async Task<CourseResponse?> GetCourseByIdAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            return course == null ? null : MapToResponse(course);
        }

        public async Task<List<CourseResponse>> GetAllCoursesAsync()
        {
            var courses = await _context.Courses
                .OrderBy(c => c.CourseCode)
                .ToListAsync();
            return courses.Select(MapToResponse).ToList();
        }

        public async Task<List<CourseResponse>> GetCoursesByDepartmentAsync(string department)
        {
            var courses = await _context.Courses
                .Where(c => c.Department == department)
                .OrderBy(c => c.CourseCode)
                .ToListAsync();
            return courses.Select(MapToResponse).ToList();
        }

        public async Task<CourseResponse?> UpdateCourseAsync(int id, UpdateCourseDto dto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.CourseCode) && dto.CourseCode != course.CourseCode)
            {
                var conflict = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == dto.CourseCode);
                if (conflict != null)
                    throw new Exception($"Course with code {dto.CourseCode} already exists");
                course.CourseCode = dto.CourseCode;
            }

            if (!string.IsNullOrWhiteSpace(dto.CourseName))
                course.CourseName = dto.CourseName;

            if (dto.Credits.HasValue)
                course.Credits = dto.Credits.Value;

            if (dto.Department != null)
                course.Department = dto.Department;

            if (dto.Description != null)
                course.Description = dto.Description;

            await _context.SaveChangesAsync();
            return MapToResponse(course);
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return false;

            var hasAssignments = await _context.CourseAssignments.AnyAsync(ca => ca.CourseId == id);
            if (hasAssignments)
                throw new Exception("Cannot delete course: it has existing lecturer assignments. Remove assignments first.");

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }

        private static CourseResponse MapToResponse(Course course)
        {
            return new CourseResponse
            {
                Id = course.Id,
                CourseCode = course.CourseCode,
                CourseName = course.CourseName,
                Credits = course.Credits,
                Department = course.Department,
                Description = course.Description,
                CreatedAt = course.CreatedAt
            };
        }
    }
}
