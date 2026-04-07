using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public class OfficeService : IOfficeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OfficeService> _logger;

        public OfficeService(ApplicationDbContext context, ILogger<OfficeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OfficeResponse> CreateOfficeAsync(CreateOfficeDto request)
        {
            // Verify user exists if provided
            if (request.StaffUserId.HasValue)
            {
                var user = await _context.Users.FindAsync(request.StaffUserId.Value);
                if (user == null)
                {
                    throw new Exception("Staff user not found");
                }

                // Check if user already has an office
                var existingOffice = await _context.Offices.FirstOrDefaultAsync(o => o.StaffUserId == request.StaffUserId.Value);
                if (existingOffice != null)
                {
                    throw new Exception($"User {user.Name} already has an office assigned");
                }
            }

            var office = new Office
            {
                OfficeName = request.OfficeName,
                OfficeNumber = request.OfficeNumber,
                Building = request.Building,
                Floor = request.Floor,
                Department = request.Department,
                PhoneExtension = request.PhoneExtension,
                StaffUserId = request.StaffUserId,
                AvailabilityStatus = request.AvailabilityStatus,
                RegularOpenTime = request.RegularOpenTime,
                RegularCloseTime = request.RegularCloseTime,
                StatusUpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Offices.Add(office);
            await _context.SaveChangesAsync();

            return await GetOfficeByIdAsync(office.Id) ?? throw new Exception("Failed to create office");
        }

        public async Task<OfficeResponse?> GetOfficeByIdAsync(int officeId)
        {
            var office = await _context.Offices
                .Include(o => o.StaffUser)
                .FirstOrDefaultAsync(o => o.Id == officeId);

            return office == null ? null : MapToResponse(office);
        }

        public async Task<List<OfficeResponse>> GetAllOfficesAsync()
        {
            var offices = await _context.Offices
                .Include(o => o.StaffUser)
                .OrderBy(o => o.Building)
                .ThenBy(o => o.Floor)
                .ThenBy(o => o.OfficeNumber)
                .ToListAsync();

            return offices.Select(MapToResponse).ToList();
        }

        public async Task<List<OfficeResponse>> GetOfficesByStatusAsync(AvailabilityStatus status)
        {
            var offices = await _context.Offices
                .Include(o => o.StaffUser)
                .Where(o => o.AvailabilityStatus == status)
                .OrderBy(o => o.Building)
                .ThenBy(o => o.Floor)
                .ThenBy(o => o.OfficeNumber)
                .ToListAsync();

            return offices.Select(MapToResponse).ToList();
        }

        public async Task<OfficeResponse?> GetOfficeByUserIdAsync(int staffUserId)
        {
            var office = await _context.Offices
                .Include(o => o.StaffUser)
                .FirstOrDefaultAsync(o => o.StaffUserId == staffUserId);

            return office == null ? null : MapToResponse(office);
        }

        public async Task<OfficeResponse?> UpdateOfficeAsync(int officeId, CreateOfficeDto request)
        {
            var office = await _context.Offices.FindAsync(officeId);
            if (office == null) return null;

            // Verify user exists if changed
            if (request.StaffUserId.HasValue && office.StaffUserId != request.StaffUserId)
            {
                var user = await _context.Users.FindAsync(request.StaffUserId.Value);
                if (user == null)
                {
                    throw new Exception("Staff user not found");
                }

                // Check if new user already has an office
                var existingOffice = await _context.Offices.FirstOrDefaultAsync(o => o.StaffUserId == request.StaffUserId.Value && o.Id != officeId);
                if (existingOffice != null)
                {
                    throw new Exception($"User already has an office assigned");
                }
            }

            office.OfficeName = request.OfficeName;
            office.OfficeNumber = request.OfficeNumber;
            office.Building = request.Building;
            office.Floor = request.Floor;
            office.Department = request.Department;
            office.PhoneExtension = request.PhoneExtension;
            office.StaffUserId = request.StaffUserId;
            office.AvailabilityStatus = request.AvailabilityStatus;
            office.RegularOpenTime = request.RegularOpenTime;
            office.RegularCloseTime = request.RegularCloseTime;
            office.StatusUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetOfficeByIdAsync(officeId);
        }

        public async Task<OfficeResponse?> UpdateOfficeAvailabilityAsync(int officeId, UpdateOfficeAvailabilityDto request)
        {
            var office = await _context.Offices.FindAsync(officeId);
            if (office == null) return null;

            office.AvailabilityStatus = request.AvailabilityStatus;
            office.StatusUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetOfficeByIdAsync(officeId);
        }

        public async Task<bool> DeleteOfficeAsync(int officeId)
        {
            var office = await _context.Offices.FindAsync(officeId);
            if (office == null) return false;

            _context.Offices.Remove(office);
            await _context.SaveChangesAsync();

            return true;
        }

        private OfficeResponse MapToResponse(Office office)
        {
            return new OfficeResponse
            {
                Id = office.Id,
                OfficeName = office.OfficeName,
                OfficeNumber = office.OfficeNumber,
                Building = office.Building,
                Floor = office.Floor,
                Department = office.Department,
                PhoneExtension = office.PhoneExtension,
                StaffUserId = office.StaffUserId,
                StaffUserName = office.StaffUser?.Name,
                StaffUserEmail = office.StaffUser?.Email,
                AvailabilityStatus = office.AvailabilityStatus.ToString(),
                RegularOpenTime = office.RegularOpenTime,
                RegularCloseTime = office.RegularCloseTime,
                StatusUpdatedAt = office.StatusUpdatedAt,
                CreatedAt = office.CreatedAt
            };
        }
    }
}
