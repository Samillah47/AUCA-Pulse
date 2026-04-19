using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;

namespace AUCAPulse.Services
{
    public interface ICourseService
    {
        Task<CourseResponse> CreateCourseAsync(CreateCourseDto dto);
        Task<CourseResponse?> GetCourseByIdAsync(int id);
        Task<List<CourseResponse>> GetAllCoursesAsync();
        Task<List<CourseResponse>> GetCoursesByDepartmentAsync(string department);
        Task<CourseResponse?> UpdateCourseAsync(int id, UpdateCourseDto dto);
        Task<bool> DeleteCourseAsync(int id);
    }
}
