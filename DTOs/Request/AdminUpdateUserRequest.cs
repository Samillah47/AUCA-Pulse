namespace AUCAPulse.DTOs.Request
{
    /// <summary>
    /// Used only by ADMIN when editing another user from the User Management
    /// page. Any null / empty value is ignored — the existing value is kept.
    /// </summary>
    public class AdminUpdateUserRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string? IdentificationNumber { get; set; }

        /// <summary>STUDENT, LECTURER, STAFF, or ADMIN.</summary>
        public string? RoleType { get; set; }

        /// <summary>PENDING, APPROVED, or REJECTED.</summary>
        public string? Status { get; set; }
    }
}
