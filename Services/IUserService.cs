using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;

namespace AUCAPulse.Services
{
    public interface IUserService
    {
        Task<UserResponse?> GetUserByIdAsync(int userId);
        Task<UserResponse?> GetUserByEmailAsync(string email);
        Task<List<UserResponse>> GetAllUsersAsync();
        Task<List<UserResponse>> GetUsersByStatusAsync(UserStatus status);
        Task<List<UserResponse>> GetUsersByRoleAsync(int roleId);
        Task<List<UserResponse>> GetUsersByRoleNameAsync(string roleName);
        Task<UserResponse?> UpdateUserAsync(int userId, UpdateUserRequest request);
        Task<UserResponse?> UpdateUserStatusAsync(int userId, UpdateUserStatusRequest request);
        Task<bool> DeleteUserAsync(int userId);
    }
}
