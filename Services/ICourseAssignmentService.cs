using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;

namespace AUCAPulse.Services
{
    public interface ICourseAssignmentService
    {
        Task<CourseAssignmentResponse> CreateAssignmentAsync(CreateCourseAssignmentDto dto);
        Task<CourseAssignmentResponse?> GetAssignmentByIdAsync(int id);
        Task<List<CourseAssignmentResponse>> GetAllAssignmentsAsync();
        Task<List<CourseAssignmentResponse>> GetAssignmentsByLecturerAsync(int lecturerId);
        Task<List<CourseAssignmentResponse>> GetAssignmentsBySemesterAsync(int semesterId);
        Task<List<CourseAssignmentResponse>> GetAssignmentsByCourseAsync(int courseId);
        Task<bool> DeleteAssignmentAsync(int id);
    }
}
