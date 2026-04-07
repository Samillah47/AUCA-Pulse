using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;

namespace AUCAPulse.Services
{
    public interface IVerificationRequestService
    {
        Task<VerificationRequestResponse> CreateRequestAsync(int userId, CreateVerificationRequestDto request);
        Task<VerificationRequestResponse?> GetRequestByIdAsync(int requestId);
        Task<List<VerificationRequestResponse>> GetAllRequestsAsync();
        Task<List<VerificationRequestResponse>> GetRequestsByUserIdAsync(int userId);
        Task<List<VerificationRequestResponse>> GetRequestsByStatusAsync(VerificationStatus status);
        Task<VerificationRequestResponse?> UpdateRequestStatusAsync(int requestId, int adminId, UpdateVerificationRequestDto request);
        Task<bool> DeleteRequestAsync(int requestId);
    }
}
