# Office Assignment Feature - Implementation Guide

**Date:** April 19, 2026  
**Developer:** Kwizera Jean Luc (26972)  
**Status:** ✅ Complete

---

## Overview

The **Office Assignment Feature** automatically assigns real offices to staff members when their account is approved. This replaces the old system that created synthetic/placeholder offices.

### Key Points

- ✅ **Real Offices Only**: Assigns existing offices from the database (`staff_user_id IS NULL`)
- ✅ **Two Approval Workflows**: Handles both UserService and VerificationRequestService approval paths
- ✅ **Consistent Logic**: Both services use identical IsStaffUser() helper for role validation
- ✅ **Fallback Logging**: Warns admin when no unassigned offices are available
- ✅ **Audit Trail**: All assignments logged with office ID, number, and staff user ID

---

## How It Works

### Flow Diagram

```
Staff User Registered
         ↓
User Submits Verification Request OR Admin Updates Status Directly
         ↓
Status Changed to APPROVED
         ↓
IsStaffUser() Check
    ├─ YES: Proceed to office assignment
    └─ NO: Skip office assignment (Lecturer, Admin, etc.)
         ↓
Check if User Already Has Office
    ├─ YES: Do nothing (already assigned)
    └─ NO: Proceed to search
         ↓
Search for Unassigned Office (staff_user_id IS NULL)
    ├─ FOUND: Assign it to user, log success
    └─ NOT FOUND: Log warning for admin
         ↓
Staff User Gets Immediate "My Office" Access
```

---

## Code Locations

### Files Modified

| File | Method | Lines | Change |
|------|--------|-------|--------|
| `Services/UserService.cs` | `UpdateUserStatusAsync()` | 108-152 | Real office assignment logic |
| `Services/UserService.cs` | `IsStaffUser()` | 194-201 | Helper method for role validation |
| `Services/VerificationRequestService.cs` | `UpdateRequestStatusAsync()` | 125-163 | Real office assignment logic |
| `Services/VerificationRequestService.cs` | `IsStaffUser()` | 233-241 | Mirror helper method |

---

## Implementation Details

### IsStaffUser() Helper Method

```csharp
private bool IsStaffUser(User user)
{
    return user?.Role?.RoleName?.Equals("STAFF", StringComparison.OrdinalIgnoreCase) ?? false;
}
```

**Why This Exists:**
- Consolidates role validation logic
- Case-insensitive comparison handles DB variations
- Null-safe: returns `false` if user or role is null
- Reusable across multiple methods

---

### Office Assignment Query

```csharp
var availableOffice = await _context.Offices
    .FirstOrDefaultAsync(o => o.StaffUserId == null);
```

**What This Does:**
1. Queries the `offices` table
2. Finds the FIRST office where `staff_user_id` is NULL (unassigned)
3. Assigns it to the staff member: `availableOffice.StaffUserId = user.Id`
4. Changes are persisted via `SaveChangesAsync()`

---

## Logging Messages

### Success Message
```
✓ Successfully assigned real office {OfficeId} (Office #{OfficeNumber}) to approved staff user {UserId}
```

**When:** Office is successfully assigned  
**Action Needed:** None - feature worked as intended

### Warning Message
```
⚠ No unassigned offices available for approved staff user {UserId}. Admin must manually assign an office through the Office Management page.
```

**When:** No unassigned offices exist in database  
**Action Needed:** Admin should:
1. Create new offices, OR
2. Unassign existing offices from other staff members, OR
3. Manually assign an office through the Office Management UI

---

## Database Schema

### Offices Table

```sql
CREATE TABLE offices (
    id INT PRIMARY KEY,
    office_name VARCHAR(255) NOT NULL,
    office_number VARCHAR(50) NOT NULL,
    building VARCHAR(255),
    floor VARCHAR(50),
    department VARCHAR(255),
    phone_extension VARCHAR(20),
    availability_status VARCHAR(50),
    regular_open_time TIME,
    regular_close_time TIME,
    status_updated_at TIMESTAMP,
    created_at TIMESTAMP,
    staff_user_id INT,  -- ← KEY FIELD: Null = Unassigned, Not Null = Assigned to Staff
    FOREIGN KEY (staff_user_id) REFERENCES users(id)
);
```

### Key Field: `staff_user_id`
- **NULL** = Office is unassigned (available for auto-assignment)
- **NOT NULL** = Office is assigned to a specific staff member

---

## Edge Cases Handled

### Case 1: User Already Has Office
```csharp
var existingOffice = await _context.Offices
    .FirstOrDefaultAsync(o => o.StaffUserId == user.Id);

if (existingOffice == null)
{
    // Only proceed if user doesn't already have an office
}
```

**Scenario:** User approved multiple times, or reassigned  
**Behavior:** Skips office assignment (user keeps existing office)

### Case 2: No Unassigned Offices
```csharp
if (availableOffice != null)
{
    // Assign it
}
else
{
    // Log warning for admin
    _logger.LogWarning("⚠ No unassigned offices available...");
}
```

**Scenario:** All real offices already assigned  
**Behavior:** Logs warning, doesn't create synthetic office

### Case 3: Non-Staff Roles
```csharp
if (IsStaffUser(user))
{
    // Only STAFF role gets office assignment
}
```

**Scenario:** Lecturer or Admin approved  
**Behavior:** Skips office assignment entirely (only STAFF gets auto-assigned offices)

---

## Testing Workflow

### Pre-Test Requirements
1. **Database Setup**: PostgreSQL running with `auca_pulse_db`
2. **Real Offices**: Create at least 2 real offices with `staff_user_id = NULL`
3. **Admin Account**: Log in as admin (`kwizerajeanluc30@gmail.com` / `123`)

### Test Scenario 1: Verification Request Approval
```
1. Register new STAFF user via verification form
2. Log in as Admin
3. Navigate to Verification Requests
4. Approve the request
5. Check: User now has office assigned (view in "My Office" page)
6. Check Logs: Should show "✓ Successfully assigned real office..."
```

### Test Scenario 2: Direct Status Update
```
1. Register new STAFF user (or use existing)
2. Log in as Admin
3. Navigate to Users Management
4. Update user status to APPROVED
5. Check: User now has office assigned
6. Check Logs: Should show "✓ Successfully assigned real office..."
```

### Test Scenario 3: No Unassigned Offices (Fallback)
```
1. Ensure all real offices already assigned (staff_user_id NOT NULL)
2. Register new STAFF user
3. Approve them (via either workflow)
4. Check Logs: Should show "⚠ No unassigned offices available..."
5. Check Dashboard: Staff sees "My Office" page but no office assigned
6. Admin must manually assign office via Office Management page
```

---

## Database Verification Query

To verify office assignments are working:

```sql
-- Show all staff users with their assigned offices
SELECT 
    u.id AS user_id,
    u.name AS staff_name,
    u.email,
    u.status,
    o.id AS office_id,
    o.office_name,
    o.office_number,
    o.building,
    o.staff_user_id
FROM users u
JOIN roles r ON r.id = u.role_id
LEFT JOIN offices o ON o.staff_user_id = u.id
WHERE r.rolename = 'STAFF'
  AND u.status = 'APPROVED'
ORDER BY u.created_at DESC;

-- Show unassigned offices (available for auto-assignment)
SELECT id, office_name, office_number, building, staff_user_id
FROM offices
WHERE staff_user_id IS NULL
ORDER BY id;
```

---

## Troubleshooting

### Problem: Staff User Approved But No Office in "My Office"

**Check 1:** Are there unassigned offices?
```sql
SELECT COUNT(*) FROM offices WHERE staff_user_id IS NULL;
```
- If 0: Create more offices in Office Management page
- If > 0: Check logs for errors

**Check 2:** Verify user role is STAFF
```sql
SELECT u.id, u.name, r.rolename FROM users u 
JOIN roles r ON r.id = u.role_id 
WHERE u.id = [USER_ID];
```
- Should show `STAFF` not `Lecturer` or `Admin`

**Check 3:** Check application logs
```
Look for: "✓ Successfully assigned real office..." or "⚠ No unassigned offices..."
```

---

## Performance Considerations

### Query Optimization

The office assignment query is simple and efficient:
```csharp
.FirstOrDefaultAsync(o => o.StaffUserId == null)
```

**Why It's Fast:**
- Indexed query (StaffUserId is foreign key)
- Only retrieves ONE office (FirstOrDefault stops after first match)
- No complex JOINs or aggregations

**Scalability:** Works fine for 1000+ offices

---

## Future Enhancements

1. **Automatic Office Distribution**: Use department/location logic for smart assignment
2. **Office Capacity Limits**: Prevent more than N staff per office
3. **Notification**: Email staff when office is assigned
4. **UI Dashboard**: Show admin a "pending auto-assignments" count
5. **Bulk Assignment**: Admin tool to assign offices to multiple staff at once

---

## References

- **UserService.cs**: Main office assignment logic (UserService)
- **VerificationRequestService.cs**: Mirror office assignment logic (Verification workflow)
- **Office.cs Model**: Office entity with StaffUserId FK
- **OFFICE_ASSIGNMENT_DATABASE_QUERIES.sql**: Verification queries

---

**Last Updated:** April 19, 2026  
**Status:** Ready for Testing ✅

