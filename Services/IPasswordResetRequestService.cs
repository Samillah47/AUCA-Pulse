using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;

namespace AUCAPulse.Services
{
    public interface IPasswordResetRequestService
    {
        Task<PasswordResetRequestResponse> CreateRequestAsync(CreatePasswordResetRequestDto dto);
        Task<PasswordResetRequestResponse?> GetRequestByIdAsync(int id);
        Task<PasswordResetRequestResponse?> GetRequestByTokenAsync(string token);
        Task<List<PasswordResetRequestResponse>> GetAllRequestsAsync();
        Task<List<PasswordResetRequestResponse>> GetRequestsByStatusAsync(RequestStatus status);
        Task<List<PasswordResetRequestResponse>> GetRequestsByUserIdAsync(int userId);
        Task<PasswordResetRequestResponse?> UpdateRequestStatusAsync(int id, UpdatePasswordResetRequestDto dto, int reviewedBy);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> DeleteRequestAsync(int id);
    }
}
