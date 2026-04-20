using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;

namespace AUCAPulse.Services
{
    public interface IGroupService
    {
        Task<GroupResponse> CreateGroupAsync(CreateGroupDto dto);
        Task<List<GroupResponse>> GetAllGroupsAsync();
        Task<bool> DeleteGroupAsync(int id);
    }
}
