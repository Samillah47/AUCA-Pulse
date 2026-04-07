using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public class SemesterService : ISemesterService
    {
        private readonly ApplicationDbContext _context;

        public SemesterService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SemesterResponse> CreateSemesterAsync(CreateSemesterDto dto)
        {
            if (dto.StartDate >= dto.EndDate)
                throw new Exception("Start date must be before end date");

            if (dto.IsCurrent)
            {
                var currentSemesters = await _context.Semesters.Where(s => s.IsCurrent).ToListAsync();
                foreach (var sem in currentSemesters)
                {
                    sem.IsCurrent = false;
                }
            }

            var semester = new Semester
            {
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsCurrent = dto.IsCurrent,
                CreatedAt = DateTime.UtcNow
            };

            _context.Semesters.Add(semester);
            await _context.SaveChangesAsync();

            return MapToResponse(semester);
        }

        public async Task<SemesterResponse?> GetSemesterByIdAsync(int id)
        {
            var semester = await _context.Semesters.FindAsync(id);
            return semester == null ? null : MapToResponse(semester);
        }

        public async Task<List<SemesterResponse>> GetAllSemestersAsync()
        {
            var semesters = await _context.Semesters
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();
            return semesters.Select(MapToResponse).ToList();
        }

        public async Task<SemesterResponse?> GetCurrentSemesterAsync()
        {
            var semester = await _context.Semesters
                .FirstOrDefaultAsync(s => s.IsCurrent);
            return semester == null ? null : MapToResponse(semester);
        }

        public async Task<SemesterResponse?> UpdateSemesterAsync(int id, UpdateSemesterDto dto)
        {
            var semester = await _context.Semesters.FindAsync(id);
            if (semester == null) return null;

            if (dto.Name != null)
                semester.Name = dto.Name;

            if (dto.StartDate.HasValue)
                semester.StartDate = dto.StartDate.Value;

            if (dto.EndDate.HasValue)
                semester.EndDate = dto.EndDate.Value;

            if (semester.StartDate >= semester.EndDate)
                throw new Exception("Start date must be before end date");

            if (dto.IsCurrent.HasValue && dto.IsCurrent.Value && !semester.IsCurrent)
            {
                var currentSemesters = await _context.Semesters.Where(s => s.IsCurrent && s.Id != id).ToListAsync();
                foreach (var sem in currentSemesters)
                {
                    sem.IsCurrent = false;
                }
                semester.IsCurrent = true;
            }
            else if (dto.IsCurrent.HasValue)
            {
                semester.IsCurrent = dto.IsCurrent.Value;
            }

            await _context.SaveChangesAsync();
            return MapToResponse(semester);
        }

        public async Task<bool> SetCurrentSemesterAsync(int id)
        {
            var semester = await _context.Semesters.FindAsync(id);
            if (semester == null) return false;

            var currentSemesters = await _context.Semesters.Where(s => s.IsCurrent).ToListAsync();
            foreach (var sem in currentSemesters)
            {
                sem.IsCurrent = false;
            }

            semester.IsCurrent = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSemesterAsync(int id)
        {
            var semester = await _context.Semesters.FindAsync(id);
            if (semester == null) return false;

            _context.Semesters.Remove(semester);
            await _context.SaveChangesAsync();
            return true;
        }

        private static SemesterResponse MapToResponse(Semester semester)
        {
            return new SemesterResponse
            {
                Id = semester.Id,
                Name = semester.Name,
                StartDate = semester.StartDate,
                EndDate = semester.EndDate,
                IsCurrent = semester.IsCurrent,
                CreatedAt = semester.CreatedAt
            };
        }
    }
}
