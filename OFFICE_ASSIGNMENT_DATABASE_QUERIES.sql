-- =====================================================================
-- OFFICE ASSIGNMENT FEATURE - DATABASE VERIFICATION QUERIES
-- =====================================================================
-- Date: April 19, 2026
-- Purpose: Verify office assignment feature is working correctly
-- Database: PostgreSQL (auca_pulse_db)
-- 
-- Usage: Copy individual queries and run in pgAdmin or psql
-- =====================================================================

-- =====================================================================
-- QUERY 1: All Approved Staff with Their Assigned Offices
-- =====================================================================
-- Shows all STAFF users who are APPROVED and their office assignments
-- Helpful for: Verifying auto-assignment worked
--
-- Expected Result:
-- - Column "office_id" should NOT be NULL for successfully assigned staff
-- - Column "office_name" should show real office names (not "AUTO-{id}" pattern)
-- =====================================================================

SELECT 
    u.id AS staff_user_id,
    u.name AS staff_name,
    u.email,
    u.department,
    u.status,
    u.created_at,
    o.id AS office_id,
    o.office_name,
    o.office_number,
    o.building,
    o.floor,
    o.availability_status,
    o.staff_user_id,
    CASE 
        WHEN o.id IS NULL THEN '⚠ NO OFFICE ASSIGNED'
        WHEN o.office_number LIKE 'AUTO-%' THEN '❌ SYNTHETIC OFFICE (OLD SYSTEM)'
        ELSE '✓ REAL OFFICE (NEW SYSTEM)'
    END AS assignment_status
FROM users u
JOIN roles r ON r.id = u.role_id
LEFT JOIN offices o ON o.staff_user_id = u.id
WHERE r.rolename = 'STAFF'
  AND u.status = 'APPROVED'
ORDER BY u.created_at DESC;

-- =====================================================================
-- QUERY 2: Unassigned Offices (Available for Auto-Assignment)
-- =====================================================================
-- Shows all offices that are NOT yet assigned to any staff member
-- Helpful for: Checking if offices are available for the next approval
--
-- Expected Result:
-- - This should return offices with staff_user_id = NULL
-- - If count is 0, admin needs to create more offices or unassign existing ones
-- =====================================================================

SELECT 
    id,
    office_name,
    office_number,
    building,
    floor,
    department,
    availability_status,
    staff_user_id,
    created_at
FROM offices
WHERE staff_user_id IS NULL
ORDER BY id;

-- Count of unassigned offices
SELECT COUNT(*) AS unassigned_office_count
FROM offices
WHERE staff_user_id IS NULL;

-- =====================================================================
-- QUERY 3: Assigned Offices (Currently Assigned to Staff)
-- =====================================================================
-- Shows all offices that ARE assigned to staff members
-- Helpful for: Seeing current assignments and potential reassignments
-- =====================================================================

SELECT 
    o.id,
    o.office_name,
    o.office_number,
    o.building,
    o.floor,
    o.staff_user_id,
    u.name AS assigned_to_staff,
    u.email,
    u.status,
    u.department
FROM offices o
LEFT JOIN users u ON u.id = o.staff_user_id
WHERE o.staff_user_id IS NOT NULL
ORDER BY o.id;

-- =====================================================================
-- QUERY 4: Pending Staff (Not Yet Approved - No Office Expected)
-- =====================================================================
-- Shows STAFF users who are still PENDING approval
-- Helpful for: Checking users waiting to be approved
--
-- Expected Result:
-- - All should have NULL office_id (not approved yet, so no office assigned)
-- - When you approve these users, they WILL get offices via the auto-assignment feature
-- =====================================================================

SELECT 
    u.id,
    u.name,
    u.email,
    u.status,
    u.created_at,
    o.id AS office_id
FROM users u
JOIN roles r ON r.id = u.role_id
LEFT JOIN offices o ON o.staff_user_id = u.id
WHERE r.rolename = 'STAFF'
  AND u.status = 'PENDING'
ORDER BY u.created_at DESC;

-- =====================================================================
-- QUERY 5: Office Assignment Summary (Statistics)
-- =====================================================================
-- Shows statistics about office assignments
-- Helpful for: Admin dashboard/reporting
--
-- Expected Result:
-- - total_offices: Total number of offices in system
-- - assigned_offices: Offices currently assigned to staff
-- - unassigned_offices: Offices available for auto-assignment
-- - total_approved_staff: Staff members approved and should have offices
-- - staff_with_offices: Staff who have offices assigned
-- - staff_without_offices: Staff who are approved but have NO office (⚠ needs manual assignment)
-- =====================================================================

SELECT 
    (SELECT COUNT(*) FROM offices) AS total_offices,
    (SELECT COUNT(*) FROM offices WHERE staff_user_id IS NOT NULL) AS assigned_offices,
    (SELECT COUNT(*) FROM offices WHERE staff_user_id IS NULL) AS unassigned_offices,
    (SELECT COUNT(*) FROM users u JOIN roles r ON r.id = u.role_id WHERE r.rolename = 'STAFF' AND u.status = 'APPROVED') AS total_approved_staff,
    (SELECT COUNT(*) FROM users u JOIN roles r ON r.id = u.role_id WHERE r.rolename = 'STAFF' AND u.status = 'APPROVED' AND u.id IN (SELECT DISTINCT staff_user_id FROM offices WHERE staff_user_id IS NOT NULL)) AS staff_with_offices,
    (SELECT COUNT(*) FROM users u JOIN roles r ON r.id = u.role_id WHERE r.rolename = 'STAFF' AND u.status = 'APPROVED' AND u.id NOT IN (SELECT DISTINCT COALESCE(staff_user_id, -1) FROM offices)) AS staff_without_offices;

-- =====================================================================
-- QUERY 6: Detailed Office Assignment Audit Trail
-- =====================================================================
-- Shows office assignments with timestamps and related info
-- Helpful for: Audit trail and understanding when offices were assigned
-- =====================================================================

SELECT 
    u.id,
    u.name,
    u.email,
    u.status,
    u.updated_at AS user_status_updated,
    o.id AS office_id,
    o.office_name,
    o.office_number,
    o.created_at AS office_created,
    CASE 
        WHEN o.office_number LIKE 'AUTO-%' THEN 'SYNTHETIC (Pre-Feature)'
        ELSE 'REAL (Post-Feature)'
    END AS office_type,
    DATEDIFF(second, u.updated_at, o.created_at) AS seconds_until_office_created
FROM users u
JOIN roles r ON r.id = u.role_id
LEFT JOIN offices o ON o.staff_user_id = u.id
WHERE r.rolename = 'STAFF'
  AND u.status = 'APPROVED'
ORDER BY u.updated_at DESC;

-- =====================================================================
-- QUERY 7: Problematic Staff (Approved but No Office)
-- =====================================================================
-- ⚠ ALERT QUERY: Shows staff who should have offices but don't
-- Helpful for: Identifying issues with auto-assignment feature
--
-- Expected Result:
-- - Should return 0 rows if feature is working correctly
-- - If any rows returned: These staff need manual office assignment
-- =====================================================================

SELECT 
    u.id,
    u.name,
    u.email,
    u.department,
    u.updated_at AS approval_date,
    'NO OFFICE ASSIGNED' AS issue
FROM users u
JOIN roles r ON r.id = u.role_id
LEFT JOIN offices o ON o.staff_user_id = u.id
WHERE r.rolename = 'STAFF'
  AND u.status = 'APPROVED'
  AND o.id IS NULL
ORDER BY u.updated_at DESC;

-- =====================================================================
-- QUERY 8: Quick Health Check (One-Liner)
-- =====================================================================
-- Single query to check overall feature health
-- Helpful for: Quick status check before/after testing
--
-- Interpretation:
-- - If office_availability_ratio is 1.0: All offices assigned (feature working, may need more offices)
-- - If office_availability_ratio is 0.0: No offices assigned (check logs for errors)
-- - If office_availability_ratio is 0.5: Half assigned, half available (normal state)
-- =====================================================================

SELECT 
    (SELECT COUNT(*) FROM offices WHERE staff_user_id IS NOT NULL)::decimal / 
    NULLIF((SELECT COUNT(*) FROM offices), 0) AS office_assignment_ratio,
    (SELECT COUNT(*) FROM users u JOIN roles r ON r.id = u.role_id WHERE r.rolename = 'STAFF' AND u.status = 'APPROVED') AS approved_staff_count,
    (SELECT COUNT(*) FROM offices WHERE staff_user_id IS NULL) AS available_offices_count;

-- =====================================================================
-- QUERY 9: Offices by Building/Department
-- =====================================================================
-- Shows office distribution across buildings and departments
-- Helpful for: Understanding office inventory and planning assignments
-- =====================================================================

SELECT 
    building,
    department,
    COUNT(*) AS total_offices,
    COUNT(CASE WHEN staff_user_id IS NOT NULL THEN 1 END) AS assigned_count,
    COUNT(CASE WHEN staff_user_id IS NULL THEN 1 END) AS unassigned_count
FROM offices
GROUP BY building, department
ORDER BY building, department;

-- =====================================================================
-- QUERY 10: Manual Assignment Check - Use This if Issues Found
-- =====================================================================
-- If you need to manually assign an office to a staff member:
--
-- STEP 1: Find an unassigned office
SELECT * FROM offices WHERE staff_user_id IS NULL LIMIT 1;
-- (Note the office ID, e.g., 5)
--
-- STEP 2: Find staff member needing assignment
SELECT * FROM users WHERE id = [STAFF_USER_ID] AND role_id = (SELECT id FROM roles WHERE rolename = 'STAFF');
-- (Note the staff user ID)
--
-- STEP 3: Run the UPDATE to manually assign
-- UPDATE offices SET staff_user_id = [STAFF_USER_ID] WHERE id = [OFFICE_ID];
--
-- =====================================================================

-- =====================================================================
-- END OF OFFICE ASSIGNMENT VERIFICATION QUERIES
-- =====================================================================
-- For issues or questions, check:
-- - Application Logs (look for ✓ or ⚠ messages)
-- - UserService.cs UpdateUserStatusAsync() method
-- - VerificationRequestService.cs UpdateRequestStatusAsync() method
-- =====================================================================

