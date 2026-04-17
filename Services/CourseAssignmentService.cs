using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public class CourseAssignmentService : ICourseAssignmentService
    {
        private readonly ApplicationDbContext _context;

        public CourseAssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CourseAssignmentResponse> CreateAssignmentAsync(CreateCourseAssignmentDto dto)
        {
            var lecturer = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == dto.LecturerId);
            if (lecturer == null)
                throw new Exception("Lecturer not found");
            if (lecturer.Role?.RoleName != "LECTURER")
                throw new Exception("Selected user is not a lecturer");

            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null)
                throw new Exception("Course not found");

            var semester = await _context.Semesters.FindAsync(dto.SemesterId);
            if (semester == null)
                throw new Exception("Semester not found");

            var duplicate = await _context.CourseAssignments.AnyAsync(ca =>
                ca.LecturerId == dto.LecturerId &&
                ca.CourseId == dto.CourseId &&
                ca.SemesterId == dto.SemesterId);
            if (duplicate)
                throw new Exception("This lecturer is already assigned to this course for the selected semester");

            var assignment = new CourseAssignment
            {
                LecturerId = dto.LecturerId,
                CourseId = dto.CourseId,
                SemesterId = dto.SemesterId,
                AssignedAt = DateTime.UtcNow
            };

            _context.CourseAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return (await GetAssignmentByIdAsync(assignment.Id))!;
        }

        public async Task<CourseAssignmentResponse?> GetAssignmentByIdAsync(int id)
        {
            var assignment = await LoadWithIncludes().FirstOrDefaultAsync(ca => ca.Id == id);
            return assignment == null ? null : MapToResponse(assignment);
        }

        public async Task<List<CourseAssignmentResponse>> GetAllAssignmentsAsync()
        {
            var list = await LoadWithIncludes()
                .OrderByDescending(ca => ca.AssignedAt)
                .ToListAsync();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<CourseAssignmentResponse>> GetAssignmentsByLecturerAsync(int lecturerId)
        {
            var list = await LoadWithIncludes()
                .Where(ca => ca.LecturerId == lecturerId)
                .OrderByDescending(ca => ca.AssignedAt)
                .ToListAsync();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<CourseAssignmentResponse>> GetAssignmentsBySemesterAsync(int semesterId)
        {
            var list = await LoadWithIncludes()
                .Where(ca => ca.SemesterId == semesterId)
                .OrderBy(ca => ca.Lecturer!.Name)
                .ToListAsync();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<CourseAssignmentResponse>> GetAssignmentsByCourseAsync(int courseId)
        {
            var list = await LoadWithIncludes()
                .Where(ca => ca.CourseId == courseId)
                .OrderByDescending(ca => ca.AssignedAt)
                .ToListAsync();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<bool> DeleteAssignmentAsync(int id)
        {
            var assignment = await _context.CourseAssignments.FindAsync(id);
            if (assignment == null) return false;

            _context.CourseAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<CourseAssignment> LoadWithIncludes()
        {
            return _context.CourseAssignments
                .Include(ca => ca.Lecturer)
                .Include(ca => ca.Course)
                .Include(ca => ca.Semester);
        }

        private static CourseAssignmentResponse MapToResponse(CourseAssignment ca)
        {
            return new CourseAssignmentResponse
            {
                Id = ca.Id,
                LecturerId = ca.LecturerId,
                LecturerName = ca.Lecturer?.Name ?? string.Empty,
                LecturerEmail = ca.Lecturer?.Email ?? string.Empty,
                CourseId = ca.CourseId,
                CourseCode = ca.Course?.CourseCode ?? string.Empty,
                CourseName = ca.Course?.CourseName ?? string.Empty,
                Credits = ca.Course?.Credits ?? 0,
                SemesterId = ca.SemesterId,
                SemesterName = ca.Semester?.Name ?? string.Empty,
                AssignedAt = ca.AssignedAt
            };
        }
    }
}
