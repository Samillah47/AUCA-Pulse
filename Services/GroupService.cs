using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public class GroupService : IGroupService
    {
        private readonly ApplicationDbContext _context;

        public GroupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GroupResponse> CreateGroupAsync(CreateGroupDto dto)
        {
            var name = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Group name is required.");

            if (await _context.Groups.AnyAsync(g => g.Name == name))
                throw new Exception($"A group named '{name}' already exists.");

            var group = new Group
            {
                Name = name,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            return MapToResponse(group);
        }

        public async Task<List<GroupResponse>> GetAllGroupsAsync()
        {
            var groups = await _context.Groups
                .OrderBy(g => g.Name)
                .ToListAsync();
            return groups.Select(MapToResponse).ToList();
        }

        public async Task<bool> DeleteGroupAsync(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null) return false;

            var inUse = await _context.CourseAssignments.AnyAsync(ca => ca.GroupId == id);
            if (inUse)
                throw new Exception("This group is used by existing course assignments. Remove those assignments first.");

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return true;
        }

        private static GroupResponse MapToResponse(Group g) => new()
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            CreatedAt = g.CreatedAt
        };
    }
}
