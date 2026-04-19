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

            var group = await _context.Groups.FindAsync(dto.GroupId);
            if (group == null)
                throw new Exception("Group not found");

            var conflict = await _context.CourseAssignments
                .Include(ca => ca.Lecturer)
                .FirstOrDefaultAsync(ca =>
                    ca.CourseId == dto.CourseId &&
                    ca.SemesterId == dto.SemesterId &&
                    ca.GroupId == dto.GroupId);
            if (conflict != null)
            {
                var existingName = conflict.Lecturer?.Name ?? "another lecturer";
                throw new Exception($"{course.CourseCode} Group {group.Name} is already assigned to {existingName} for this semester.");
            }

            var assignment = new CourseAssignment
            {
                LecturerId = dto.LecturerId,
                CourseId = dto.CourseId,
                SemesterId = dto.SemesterId,
                GroupId = dto.GroupId,
                AssignedAt = DateTime.UtcNow
            };

            _context.CourseAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return (await GetAssignmentByIdAsync(assignment.Id))!;
        }

        public async Task<(int copied, int skipped)> CopyAssignmentsFromSemesterAsync(int sourceSemesterId, int targetSemesterId)
        {
            if (sourceSemesterId == targetSemesterId)
                throw new Exception("Source and target semesters must be different.");

            var source = await _context.Semesters.FindAsync(sourceSemesterId)
                ?? throw new Exception("Source semester not found.");
            var target = await _context.Semesters.FindAsync(targetSemesterId)
                ?? throw new Exception("Target semester not found.");

            var sourceAssignments = await _context.CourseAssignments
                .Where(ca => ca.SemesterId == sourceSemesterId)
                .ToListAsync();

            if (sourceAssignments.Count == 0)
                throw new Exception($"'{source.Name}' has no assignments to copy.");

            // Preload existing (course, group) pairs in the target semester so
            // we skip duplicates without triggering the unique index.
            var existingTargetKeys = await _context.CourseAssignments
                .Where(ca => ca.SemesterId == targetSemesterId)
                .Select(ca => new { ca.CourseId, ca.GroupId })
                .ToListAsync();
            var existingSet = new HashSet<(int, int)>(existingTargetKeys.Select(x => (x.CourseId, x.GroupId)));

            var toInsert = new List<CourseAssignment>();
            var skipped = 0;

            foreach (var src in sourceAssignments)
            {
                if (existingSet.Contains((src.CourseId, src.GroupId)))
                {
                    skipped++;
                    continue;
                }
                toInsert.Add(new CourseAssignment
                {
                    LecturerId = src.LecturerId,
                    CourseId = src.CourseId,
                    GroupId = src.GroupId,
                    SemesterId = targetSemesterId,
                    AssignedAt = DateTime.UtcNow
                });
                existingSet.Add((src.CourseId, src.GroupId));
            }

            if (toInsert.Count > 0)
            {
                _context.CourseAssignments.AddRange(toInsert);
                await _context.SaveChangesAsync();
            }

            return (toInsert.Count, skipped);
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
                .Include(ca => ca.Semester)
                .Include(ca => ca.Group);
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
                GroupId = ca.GroupId,
                GroupName = ca.Group?.Name ?? string.Empty,
                AssignedAt = ca.AssignedAt
            };
        }
    }
}
