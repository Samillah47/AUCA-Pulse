# Office Assignment Feature - Testing Checklist

**Date:** April 19, 2026  
**Version:** 1.0  
**Feature:** Auto-assign real offices to approved STAFF users

---

## Pre-Test Setup ✅

### Environment Check

- [ ] PostgreSQL database running (`auca_pulse_db`)
- [ ] Application built and running (`http://localhost:5113`)
- [ ] Admin logged in (`kwizerajeanluc30@gmail.com` / `123`)
- [ ] Database has at least 2 real offices with `staff_user_id = NULL`
- [ ] Application logs accessible (check output/console)

### Database Preparation

```powershell
# Check unassigned offices
psql -U postgres -d auca_pulse_db -c "SELECT COUNT(*) FROM offices WHERE staff_user_id IS NULL;"
# Should return >= 2

# If needed, clear old test data:
# psql -U postgres -d auca_pulse_db -c "UPDATE offices SET staff_user_id = NULL WHERE office_number LIKE 'AUTO-%';"
```

---

## TEST SCENARIO 1: Verification Request Approval Path ✅

**Objective:** Verify office auto-assignment via Verification Request workflow  
**Role:** Staff member → verification required → admin approves → office assigned

### Steps

| # | Action | Expected Result | ✓ |
|---|--------|-----------------|---|
| 1 | Register new user with STAFF role via public signup | User created in PENDING status | [ ] |
| 2 | Submit verification request (with ID document) | Verification request in PENDING | [ ] |
| 3 | Log in as Admin | Dashboard shows pending verifications | [ ] |
| 4 | Navigate to Verification Requests page | Request visible in list | [ ] |
| 5 | Click "Approve" on the request | Status changes to APPROVED | [ ] |
| 6 | Check application logs | Should see "✓ Successfully assigned real office {id} (Office #{number}) to verified staff user {id}" | [ ] |
| 7 | Log out and back in as the staff user | Can access staff dashboard | [ ] |
| 8 | Click "View Office" on dashboard | Office details displayed (NOT "No office assigned") | [ ] |
| 9 | Check office details page | Shows real office name, number, building, floor | [ ] |
| 10 | Query database with Query #1 | New staff user appears with assigned office (not AUTO- office) | [ ] |

### Verification Query
```sql
-- Run this after approval to verify office was assigned
SELECT u.name, u.email, o.office_name, o.office_number
FROM users u
LEFT JOIN offices o ON o.staff_user_id = u.id
WHERE u.email = '[NEW_STAFF_EMAIL]'
  AND u.role_id = (SELECT id FROM roles WHERE rolename = 'STAFF');
```

**Pass Criteria:** Office_name and office_number are NOT NULL, office_number does NOT start with "AUTO-"

---

## TEST SCENARIO 2: Direct User Status Update Path ✅

**Objective:** Verify office auto-assignment via direct status update (admin panel)  
**Role:** Admin directly approves user without verification flow

### Steps

| # | Action | Expected Result | ✓ |
|---|--------|-----------------|---|
| 1 | Register new user with STAFF role | User in PENDING status | [ ] |
| 2 | Log in as Admin | Admin dashboard loads | [ ] |
| 3 | Navigate to Users Management page | List of users displayed | [ ] |
| 4 | Search for the new staff user | User found in list | [ ] |
| 5 | Click "Edit" or "Update Status" on the user | Edit form opens | [ ] |
| 6 | Change Status from PENDING to APPROVED | Dropdown shows APPROVED option | [ ] |
| 7 | Click "Save" or "Approve" button | Form submits successfully | [ ] |
| 8 | Check application logs | Should see "✓ Successfully assigned real office..." message | [ ] |
| 9 | Navigate back to Users list | Status now shows APPROVED | [ ] |
| 10 | Log out and back in as staff user | Can access staff dashboard | [ ] |
| 11 | Click "View Office" | Office details shown with real office info | [ ] |
| 12 | Query database | Office confirmed assigned to user | [ ] |

### Verification Query
```sql
-- Verify office was assigned after direct status update
SELECT u.name, u.status, o.office_name, o.office_number
FROM users u
JOIN roles r ON r.id = u.role_id
LEFT JOIN offices o ON o.staff_user_id = u.id
WHERE r.rolename = 'STAFF' AND u.status = 'APPROVED'
ORDER BY u.updated_at DESC LIMIT 1;
```

**Pass Criteria:** Most recent approved staff has non-NULL office_name and office_number

---

## TEST SCENARIO 3: Fallback - No Unassigned Offices ⚠️ ✅

**Objective:** Verify fallback behavior when no real offices available  
**Role:** All offices assigned → new staff approved → warning logged

### Setup

```sql
-- Before this test, assign all offices to staff:
UPDATE offices SET staff_user_id = (SELECT id FROM users WHERE id = 1 LIMIT 1) WHERE staff_user_id IS NULL;
-- (Adjust to match your setup)

-- Verify all offices now have assignments:
SELECT COUNT(*) FROM offices WHERE staff_user_id IS NULL;
-- Should return 0
```

### Steps

| # | Action | Expected Result | ✓ |
|---|--------|-----------------|---|
| 1 | Ensure ALL offices are assigned (see setup above) | No offices with staff_user_id = NULL | [ ] |
| 2 | Register new STAFF user | User in PENDING | [ ] |
| 3 | Approve the user (via either workflow) | User status: APPROVED | [ ] |
| 4 | Check application logs | Should see "⚠ No unassigned offices available for approved staff user..." | [ ] |
| 5 | Log in as staff user | Staff dashboard loads | [ ] |
| 6 | Go to "My Office" page | Shows "No office assigned" or similar message | [ ] |
| 7 | Log in as Admin | Admin dashboard | [ ] |
| 8 | Check office management | See options to manually assign office to staff | [ ] |
| 9 | Manually assign an office to the staff user | Office assignment form/modal works | [ ] |
| 10 | Staff logs back in | "My Office" now shows the assigned office | [ ] |

### Verification Query
```sql
-- Check for unassigned offices
SELECT COUNT(*) AS unassigned_count FROM offices WHERE staff_user_id IS NULL;
-- Should be 0 or very low

-- Check for staff without offices
SELECT u.name, u.email FROM users u
JOIN roles r ON r.id = u.role_id
WHERE r.rolename = 'STAFF' AND u.status = 'APPROVED'
  AND u.id NOT IN (SELECT DISTINCT staff_user_id FROM offices WHERE staff_user_id IS NOT NULL);
```

**Pass Criteria:** Staff without offices exist, logs show warning message, admin can manually assign offices

---

## TEST SCENARIO 4: Non-Staff Roles Not Affected ✅

**Objective:** Verify LECTURER and ADMIN roles do NOT get auto-assigned offices

### Steps

| # | Action | Expected Result | ✓ |
|---|--------|-----------------|---|
| 1 | Register new user with LECTURER role | User in PENDING | [ ] |
| 2 | Approve the LECTURER user | Status: APPROVED | [ ] |
| 3 | Check application logs | NO message about office assignment (not staff role) | [ ] |
| 4 | Query database with Query #1 | Lecturer does NOT appear in results (office assignment query is STAFF-only) | [ ] |
| 5 | Repeat with ADMIN role | Same results (no office assignment) | [ ] |
| 6 | Create an ADMIN user and approve | ADMIN has no office assigned | [ ] |

### Verification Query
```sql
-- Verify only STAFF gets office assignment (not Lecturer/Admin)
SELECT u.name, r.rolename, o.office_name
FROM users u
JOIN roles r ON r.id = u.role_id
LEFT JOIN offices o ON o.staff_user_id = u.id
WHERE u.status = 'APPROVED'
ORDER BY r.rolename;
-- Only STAFF rows should have office_name populated
```

**Pass Criteria:** Only STAFF users have offices assigned. Lecturers and Admins remain without offices.

---

## TEST SCENARIO 5: Helper Method Works Correctly ✅

**Objective:** Verify `IsStaffUser()` helper method logic  
**Technical:** Check role validation is working

### Expected Behavior

The `IsStaffUser(User user)` method should:
- Return `true` for users with role "STAFF" (case-insensitive)
- Return `false` for users with role "LECTURER" or "ADMIN"
- Return `false` if user is null
- Return `false` if user.Role is null

### Manual Verification

Since this is private method, verify through integration:

| # | Test Case | Expected | ✓ |
|---|-----------|----------|---|
| 1 | Approve STAFF user → office assigned | TRUE (office found and assigned) | [ ] |
| 2 | Approve LECTURER user → no office logic runs | FALSE (logs show no attempt to assign) | [ ] |
| 3 | Approve ADMIN user → no office logic runs | FALSE (logs show no attempt to assign) | [ ] |

**Pass Criteria:** Logs consistently show office assignment ONLY for STAFF role

---

## TEST SCENARIO 6: Edge Case - Duplicate Approval ✅

**Objective:** Verify system handles if same staff approved multiple times  
**Result:** Should not create duplicate office assignments

### Steps

| # | Action | Expected Result | ✓ |
|---|--------|-----------------|---|
| 1 | Approve STAFF user first time | Office assigned (office_1) | [ ] |
| 2 | Try to approve same user again (if possible) OR manually approve twice | System detects existing office | [ ] |
| 3 | Check application logs | NO new "Successfully assigned" message (since office already exists) | [ ] |
| 4 | Query database | Staff user still assigned to original office_1 (not a new office) | [ ] |
| 5 | Verify available offices count | Unchanged (no duplicate assignment) | [ ] |

### Verification Query
```sql
-- Check staff doesn't have multiple offices
SELECT staff_user_id, COUNT(*) as office_count
FROM offices
GROUP BY staff_user_id
HAVING COUNT(*) > 1;
-- Should return 0 rows (no staff with multiple offices)
```

**Pass Criteria:** Staff maintains single office assignment. No duplicates created.

---

## TEST SCENARIO 7: Database Transaction Integrity ✅

**Objective:** Verify changes are properly committed to database  
**Result:** Office assignments persist across app restarts

### Steps

| # | Action | Expected Result | ✓ |
|---|--------|-----------------|---|
| 1 | Approve new STAFF user | Office assigned, logged | [ ] |
| 2 | Query database immediately | Office assignment visible | [ ] |
| 3 | Shut down application completely | App stops | [ ] |
| 4 | Restart application | App starts fresh | [ ] |
| 5 | Staff logs in | Office still assigned (persisted to DB) | [ ] |
| 6 | Query database again | Office assignment still there | [ ] |

**Pass Criteria:** Office assignments persist across application restart (SaveChangesAsync() working correctly)

---

## TEST SCENARIO 8: Logging Audit Trail ✅

**Objective:** Verify all office assignments are properly logged  
**Result:** Admin can audit who assigned which offices and when

### Steps

| # | Action | Expected Result | ✓ |
|---|--------|-----------------|---|
| 1 | Approve 3 different STAFF users | 3 offices assigned | [ ] |
| 2 | Check application logs | 3 log messages with format: "✓ Successfully assigned real office {id} (Office #{number}) to approved staff user {id}" | [ ] |
| 3 | Verify log includes details | Office ID, Office Number, Staff User ID all present in logs | [ ] |
| 4 | Check log timestamps | Timestamps match approval times | [ ] |
| 5 | Test warning scenario (no offices) | Log warning message present | [ ] |
| 6 | Export/Archive logs | Can save logs for compliance | [ ] |

**Pass Criteria:** All actions properly logged with sufficient detail for audit trail

---

## TEST SCENARIO 9: My Office Page Integration ✅

**Objective:** Verify office assignment works end-to-end with "My Office" UI

### Steps

| # | Action | Expected Result | ✓ |
|---|--------|-----------------|---|
| 1 | Staff without office visits "My Office" | Shows "No office assigned" message | [ ] |
| 2 | Admin approves staff user | Office auto-assigned | [ ] |
| 3 | Staff refreshes page | "My Office" now shows office details | [ ] |
| 4 | Check office details display | Shows: office name, number, building, floor, etc. | [ ] |
| 5 | Try to update office settings | Can modify availability, hours, etc. | [ ] |
| 6 | Log out and back in | Office still visible and correct | [ ] |

**Pass Criteria:** UI smoothly shows office after approval. No "stale" data or refresh issues.

---

## TEST SCENARIO 10: Performance Under Load ✅

**Objective:** Verify feature performs well with multiple approvals  
**Result:** No slowdowns or timeout issues

### Steps

| # | Action | Expected Result | ✓ |
|---|--------|-----------------|---|
| 1 | Create 10 STAFF users | Users ready in PENDING status | [ ] |
| 2 | Approve them in rapid succession | All approvals succeed quickly (< 1s each) | [ ] |
| 3 | Monitor application logs | No timeout errors or exceptions | [ ] |
| 4 | Check database | All 10 offices assigned correctly | [ ] |
| 5 | Query database with Query #8 | Statistics show all assignments successful | [ ] |
| 6 | Test with 50+ offices | Still performs well with many offices | [ ] |

### Query for Performance Check
```sql
-- Check if query completes quickly
SELECT COUNT(*) 
FROM offices 
WHERE staff_user_id IS NULL;
-- Should return in < 100ms even with many offices
```

**Pass Criteria:** No performance degradation. All approvals successful regardless of office count.

---

## Post-Test Cleanup ✅

After completing all tests, clean up test data:

```sql
-- Delete synthetic (old) offices if any remain
DELETE FROM offices WHERE office_number LIKE 'AUTO-%';

-- Reset test staff users (optional)
DELETE FROM users 
WHERE email LIKE '%test%' 
  AND role_id = (SELECT id FROM roles WHERE rolename = 'STAFF');

-- Verify all offices back in good state
SELECT COUNT(*) FROM offices;
SELECT COUNT(*) FROM offices WHERE staff_user_id IS NULL;
```

---

## Summary Results Table

| Scenario | Status | Notes | Pass/Fail |
|----------|--------|-------|-----------|
| Verification Request Approval | [ ] | | [ ] |
| Direct Status Update | [ ] | | [ ] |
| Fallback (No Offices) | [ ] | | [ ] |
| Non-Staff Roles | [ ] | | [ ] |
| Helper Method | [ ] | | [ ] |
| Duplicate Approval | [ ] | | [ ] |
| Transaction Integrity | [ ] | | [ ] |
| Logging Audit Trail | [ ] | | [ ] |
| My Office Integration | [ ] | | [ ] |
| Performance Under Load | [ ] | | [ ] |

**OVERALL RESULT:** [ ] PASS [ ] FAIL

---

## Issues Found

Document any issues during testing:

```
Issue #1:
- Scenario: [Which test scenario]
- Description: [What went wrong]
- Steps to Reproduce: [How to recreate]
- Expected: [What should happen]
- Actual: [What actually happened]
- Severity: [Critical / High / Medium / Low]
```

---

## Sign-Off

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Developer | | | |
| QA | | | |
| Admin Approval | | | |

---

## References

- **OFFICE_ASSIGNMENT_FEATURE.md** - Implementation details
- **OFFICE_ASSIGNMENT_DATABASE_QUERIES.sql** - Verification queries
- **Services/UserService.cs** - Main implementation
- **Services/VerificationRequestService.cs** - Parallel implementation
- **Application Logs** - Check for success/warning messages

---

**Last Updated:** April 19, 2026  
**Next Review:** After all tests complete

