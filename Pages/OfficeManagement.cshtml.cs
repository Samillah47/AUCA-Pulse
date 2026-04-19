using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AUCAPulse.Pages
{
    /// <summary>
    /// Office Management Page Model
    /// Purpose: Allow ADMIN users to manage offices in the system
    /// Features: View, Create, Edit, Delete offices
    /// Date: April 19, 2026
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    public class OfficeManagementModel : PageModel
    {
        private readonly IOfficeService _officeService;
        private readonly ILogger<OfficeManagementModel> _logger;

        public OfficeManagementModel(IOfficeService officeService, ILogger<OfficeManagementModel> logger)
        {
            _officeService = officeService;
            _logger = logger;
        }

        // ===============================================================
        // PUBLIC PROPERTIES - Used by the Razor page to display data
        // ===============================================================

        /// <summary>
        /// List of all offices in the system
        /// Populated in OnGetAsync() method
        /// </summary>
        public List<OfficeResponse> Offices { get; set; } = new();

        /// <summary>
        /// DTO for creating new offices
        /// Bound from form data in OnPostCreateAsync
        /// </summary>
        [BindProperty]
        public CreateOfficeDto CreateOfficeRequest { get; set; } = new();

        /// <summary>
        /// DTO for editing existing offices
        /// Used in OnPostEditAsync
        /// </summary>
        [BindProperty]
        public UpdateOfficeDto EditOfficeRequest { get; set; } = new();

        /// <summary>
        /// Success message displayed to user
        /// </summary>
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// Error message displayed to user
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// ID of the office being edited (for modal identification)
        /// </summary>
        public int? EditingOfficeId { get; set; }

        // ===============================================================
        // PAGE LOAD - GET REQUEST
        // ===============================================================

        /// <summary>
        /// Load all offices when page is requested (GET)
        /// Called automatically by ASP.NET Core when user navigates to /OfficeManagement
        /// </summary>
        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading all offices for admin");
                Offices = await _officeService.GetAllOfficesAsync();
                _logger.LogInformation($"Successfully loaded {Offices.Count} offices");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading offices: {ex.Message}");
                ErrorMessage = "Failed to load offices. Please try again.";
            }
        }

        // ===============================================================
        // CREATE OFFICE - POST REQUEST
        // ===============================================================

        /// <summary>
        /// Create a new office
        /// Called when admin submits the "Add Office" form
        /// </summary>
        public async Task<IActionResult> OnPostCreateAsync()
        {
            try
            {
                // Validate form data
                if (string.IsNullOrEmpty(CreateOfficeRequest.OfficeName))
                {
                    ErrorMessage = "Office Name is required";
                    await OnGetAsync(); // Reload offices
                    return Page();
                }

                if (string.IsNullOrEmpty(CreateOfficeRequest.OfficeNumber))
                {
                    ErrorMessage = "Office Number is required";
                    await OnGetAsync(); // Reload offices
                    return Page();
                }

                _logger.LogInformation($"Creating new office: {CreateOfficeRequest.OfficeName}");

                // Call service to create office
                var result = await _officeService.CreateOfficeAsync(CreateOfficeRequest);

                _logger.LogInformation($"✓ Office created successfully: {result.OfficeName}");
                SuccessMessage = $"Office '{CreateOfficeRequest.OfficeName}' created successfully!";

                // Clear the form
                CreateOfficeRequest = new();

                // Reload all offices to show new one
                await OnGetAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating office: {ex.Message}");
                ErrorMessage = $"Failed to create office: {ex.Message}";
                await OnGetAsync(); // Reload offices
                return Page();
            }
        }

        // ===============================================================
        // EDIT OFFICE - POST REQUEST
        // ===============================================================

        /// <summary>
        /// Update an existing office
        /// Called when admin submits the "Edit Office" form
        /// </summary>
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostEditAsync(int officeId)
        {
            try
            {
                // Validate form data
                if (string.IsNullOrEmpty(EditOfficeRequest.OfficeName))
                {
                    ErrorMessage = "Office Name is required";
                    await OnGetAsync();
                    return Page();
                }

                if (string.IsNullOrEmpty(EditOfficeRequest.OfficeNumber))
                {
                    ErrorMessage = "Office Number is required";
                    await OnGetAsync();
                    return Page();
                }

                _logger.LogInformation($"Updating office {officeId}");

                // Call service to update office
                var result = await _officeService.UpdateOfficeAsync(officeId, EditOfficeRequest);

                _logger.LogInformation($"✓ Office updated successfully: {result.OfficeName}");
                SuccessMessage = $"Office '{EditOfficeRequest.OfficeName}' updated successfully!";

                // Reload all offices to show changes
                await OnGetAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating office: {ex.Message}");
                ErrorMessage = $"Failed to update office: {ex.Message}";
                await OnGetAsync();
                return Page();
            }
        }

        // ===============================================================
        // DELETE OFFICE - POST REQUEST
        // ===============================================================

        /// <summary>
        /// Delete an office
        /// Called when admin clicks "Delete" button for an office
        /// </summary>
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int officeId)
        {
            try
            {
                _logger.LogInformation($"Deleting office {officeId}");

                // Get office name before deletion (for success message)
                var office = Offices.FirstOrDefault(o => o.Id == officeId);
                var officeName = office?.OfficeName ?? "Office";

                // Call service to delete office
                var success = await _officeService.DeleteOfficeAsync(officeId);

                if (success)
                {
                    _logger.LogInformation($"✓ Office deleted successfully: {officeName}");
                    SuccessMessage = $"Office '{officeName}' deleted successfully!";
                }
                else
                {
                    _logger.LogWarning($"Failed to delete office {officeId}");
                    ErrorMessage = "Failed to delete office. Please try again.";
                }

                // Reload all offices
                await OnGetAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting office: {ex.Message}");
                ErrorMessage = $"Failed to delete office: {ex.Message}";
                await OnGetAsync();
                return Page();
            }
        }
    }
}

