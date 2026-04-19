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

        public async Task<BulkCourseAssignmentResult> CreateBulkAssignmentsAsync(BulkCreateCourseAssignmentDto dto)
        {
            var result = new BulkCourseAssignmentResult();

            var course = await _context.Courses.FindAsync(dto.CourseId)
                ?? throw new Exception("Course not found.");
            var semester = await _context.Semesters.FindAsync(dto.SemesterId)
                ?? throw new Exception("Semester not found.");

            if (dto.LecturerIds == null || dto.LecturerIds.Count == 0)
                throw new Exception("Please select at least one lecturer.");

            var distinctIds = dto.LecturerIds.Distinct().ToList();

            var lecturers = await _context.Users
                .Include(u => u.Role)
                .Where(u => distinctIds.Contains(u.Id))
                .ToListAsync();

            var existing = await _context.CourseAssignments
                .Where(ca => ca.CourseId == dto.CourseId && ca.SemesterId == dto.SemesterId)
                .Select(ca => ca.LecturerId)
                .ToListAsync();

            var toInsert = new List<CourseAssignment>();

            foreach (var lecturerId in distinctIds)
            {
                var lecturer = lecturers.FirstOrDefault(u => u.Id == lecturerId);
                if (lecturer == null || lecturer.Role?.RoleName != "LECTURER")
                {
                    result.InvalidLecturers++;
                    result.Notes.Add($"Skipped user id {lecturerId}: not a valid lecturer.");
                    continue;
                }

                if (existing.Contains(lecturerId))
                {
                    result.SkippedDuplicates++;
                    result.Notes.Add($"Skipped {lecturer.Name}: already assigned to this course for the selected semester.");
                    continue;
                }

                toInsert.Add(new CourseAssignment
                {
                    LecturerId = lecturerId,
                    CourseId = dto.CourseId,
                    SemesterId = dto.SemesterId,
                    AssignedAt = DateTime.UtcNow
                });
            }

            if (toInsert.Count > 0)
            {
                _context.CourseAssignments.AddRange(toInsert);
                await _context.SaveChangesAsync();

                var insertedIds = toInsert.Select(t => t.Id).ToList();
                var loaded = await LoadWithIncludes()
                    .Where(ca => insertedIds.Contains(ca.Id))
                    .ToListAsync();

                result.Assignments = loaded.Select(MapToResponse).ToList();
                result.Created = loaded.Count;
            }

            return result;
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
