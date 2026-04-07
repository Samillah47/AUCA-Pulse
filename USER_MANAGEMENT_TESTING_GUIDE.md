# User Management API Testing Guide

## Overview
This guide covers testing the User Management endpoints for AUCA Pulse system.

## Prerequisites
1. Application running on `http://localhost:5204` (or your configured port)
2. Postman installed
3. JWT token from login (stored in `{{jwt_token}}` environment variable)

## Admin Credentials
- **Email**: habiyaadolphe19@gmail.com
- **Password**: Mugisha1234!@

## Setup Steps

### 1. Import Postman Collection
- Import `AUCA_Pulse_UserManagement.postman_collection.json`
- Import `AUCA_Pulse_Local.postman_environment.json` (if not already imported)

### 2. Get JWT Token
Use the authentication endpoints to login:
```
POST http://localhost:5204/api/auth/login
Body: {
  "email": "habiyaadolphe19@gmail.com",
  "password": "Mugisha1234!@"
}
```
Then verify OTP (check console for OTP code):
```
POST http://localhost:5204/api/auth/verify-otp
Body: {
  "email": "habiyaadolphe19@gmail.com",
  "otp": "123456"
}
```
Copy the `token` from response and save it in Postman environment as `jwt_token`.

## API Endpoints

### 1. Get User by ID
**GET** `/api/user/{userId}`
- **Auth**: Required (Bearer Token)
- **Role**: Any authenticated user
- **Response**: User details

### 2. Get User by Email
**GET** `/api/user/email/{email}`
- **Auth**: Required (Bearer Token)
- **Role**: Any authenticated user
- **Example**: `/api/user/email/habiyaadolphe19@gmail.com`

### 3. Get All Users
**GET** `/api/user`
- **Auth**: Required (Bearer Token)
- **Role**: ADMIN only
- **Response**: List of all users

### 4. Get Users by Status
**GET** `/api/user/status/{status}`
- **Auth**: Required (Bearer Token)
- **Role**: ADMIN only
- **Status Values**: PENDING, APPROVED, REJECTED
- **Example**: `/api/user/status/PENDING`

### 5. Get Users by Role
**GET** `/api/user/role/{roleId}`
- **Auth**: Required (Bearer Token)
- **Role**: ADMIN only
- **Example**: `/api/user/role/{role-guid}`

### 6. Update User Profile
**PUT** `/api/user/{userId}`
- **Auth**: Required (Bearer Token)
- **Role**: User can update own profile, ADMIN can update any
- **Body**:
```json
{
  "name": "Updated Name",
  "phoneNumber": "+250788123456",
  "department": "Computer Science",
  "locationId": null,
  "officeId": null
}
```

### 7. Update User Status (Approve/Reject)
**PUT** `/api/user/{userId}/status`
- **Auth**: Required (Bearer Token)
- **Role**: ADMIN only
- **Body**:
```json
{
  "status": "APPROVED",
  "reason": "All requirements met"
}
```
- **Status Values**: PENDING, APPROVED, REJECTED
- **Note**: Sends email notification to user

### 8. Delete User
**DELETE** `/api/user/{userId}`
- **Auth**: Required (Bearer Token)
- **Role**: ADMIN only

## Testing Workflow

### Test 1: Get Admin User Details
1. Login as admin and get JWT token
2. Get roles list: `GET /api/test/roles`
3. Find ADMIN role ID
4. Get users by role: `GET /api/user/role/{admin-role-id}`
5. Get admin user by email: `GET /api/user/email/habiyaadolphe19@gmail.com`

### Test 2: Register New User and Approve
1. Register new user: `POST /api/auth/register`
```json
{
  "name": "Test User",
  "email": "testuser@auca.ac.rw",
  "password": "Test123!@",
  "identificationNumber": "TEST001",
  "roleId": "{student-role-id}",
  "department": "Computer Science"
}
```
2. Login as admin
3. Get pending users: `GET /api/user/status/PENDING`
4. Approve user: `PUT /api/user/{user-id}/status`
```json
{
  "status": "APPROVED",
  "reason": "Verified credentials"
}
```
5. Check user received approval email (console logs)

### Test 3: Update User Profile
1. Login as the test user
2. Get user details: `GET /api/user/{user-id}`
3. Update profile: `PUT /api/user/{user-id}`
```json
{
  "name": "Test User Updated",
  "phoneNumber": "+250788123456",
  "department": "Information Technology",
  "locationId": null,
  "officeId": null
}
```
4. Verify changes: `GET /api/user/{user-id}`

### Test 4: Authorization Tests
1. Try to access admin endpoints without admin role (should fail with 403)
2. Try to update another user's profile without admin role (should fail with 403)
3. Try to access endpoints without JWT token (should fail with 401)

## Expected Responses

### Success Response (Get User)
```json
{
  "id": "guid",
  "name": "User Name",
  "email": "user@auca.ac.rw",
  "identificationNumber": "ID001",
  "phoneNumber": "+250788123456",
  "department": "Computer Science",
  "status": "APPROVED",
  "role": "STUDENT",
  "roleId": "guid",
  "locationId": null,
  "locationName": null,
  "officeId": null,
  "officeName": null,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z"
}
```

### Error Responses
- **401 Unauthorized**: Missing or invalid JWT token
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: User not found
- **400 Bad Request**: Invalid request data

## Notes
- All endpoints require JWT authentication
- Admin-only endpoints are marked with "(Admin)" in the endpoint name
- Users can only update their own profiles unless they are admins
- Status updates trigger email notifications
- All timestamps are in UTC
