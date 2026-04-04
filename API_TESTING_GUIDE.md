# AUCA Pulse API Testing Guide

## Base URL
```
http://localhost:5000/api
https://localhost:5001/api
```

## Test Endpoints

### 1. Health Check
**GET** `/api/test/health`

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2026-04-04T12:00:00Z",
  "message": "AUCA Pulse API is running"
}
```

---

### 2. Check Database Connection
**GET** `/api/test/database`

**Response:**
```json
{
  "status": "connected",
  "database": "auca_pulse_db",
  "tables": {
    "roles": 4,
    "users": 1,
    "locations": 8,
    "rooms": 6,
    "offices": 4
  }
}
```

---

### 3. Check Admin User
**GET** `/api/test/admin`

**Response:**
```json
{
  "id": 1,
  "name": "System Admin",
  "email": "admin@auca.ac.rw",
  "role": "ADMIN",
  "status": "APPROVED",
  "identificationNumber": "ADMIN001",
  "department": "Administration",
  "createdAt": "2026-04-04T12:00:00Z"
}
```

---

### 4. Get All Roles
**GET** `/api/test/roles`

**Response:**
```json
[
  {
    "id": 1,
    "roleName": "STUDENT",
    "description": "Student user with search and view permissions"
  },
  {
    "id": 2,
    "roleName": "LECTURER",
    "description": "Lecturer with room occupation and status update permissions"
  },
  {
    "id": 3,
    "roleName": "STAFF",
    "description": "Staff member with office management permissions"
  },
  {
    "id": 4,
    "roleName": "ADMIN",
    "description": "Administrator with full system access"
  }
]
```

---

### 5. Get All Locations
**GET** `/api/test/locations`

**Response:**
```json
[
  {
    "id": 1,
    "name": "Kigali",
    "code": "KGL",
    "type": "PROVINCE",
    "parentId": null,
    "createdAt": "2024-01-01T00:00:00Z"
  },
  ...
]
```

---

### 6. Get All Rooms
**GET** `/api/test/rooms`

**Response:**
```json
[
  {
    "id": 1,
    "roomNumber": "A-101",
    "roomName": "Lecture Hall 1",
    "capacity": 100,
    "building": "Academic Block A",
    "floor": "1st Floor",
    "roomType": "LECTURE_HALL",
    "status": "AVAILABLE",
    "currentLecturerId": null,
    "occupiedAt": null,
    "occupiedUntil": null,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": null
  },
  ...
]
```

---

## Authentication Endpoints

### 7. Register New User
**POST** `/api/auth/register`

**Request Body:**
```json
{
  "name": "John Doe",
  "email": "john.doe@auca.ac.rw",
  "password": "Password123!",
  "identificationNumber": "STU001",
  "roleType": "STUDENT",
  "phoneNumber": "+250788123456",
  "department": "Computer Science",
  "locationId": 1
}
```

**Response:**
```json
{
  "message": "Registration submitted. Awaiting admin approval.",
  "userId": 2
}
```

---

### 8. Login (Step 1 - Get OTP)
**POST** `/api/auth/login`

**Request Body:**
```json
{
  "email": "admin@auca.ac.rw",
  "password": "Admin123!"
}
```

**Response:**
```json
{
  "status": "PENDING_OTP",
  "message": "OTP sent to email"
}
```

**Note:** Check the console output for the OTP code (6 digits)

---

### 9. Verify OTP (Step 2 - Get JWT Token)
**POST** `/api/auth/verify-otp`

**Request Body:**
```json
{
  "email": "admin@auca.ac.rw",
  "otp": "123456"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "role": "ADMIN",
  "userId": 1,
  "name": "System Admin",
  "email": "admin@auca.ac.rw",
  "status": "APPROVED"
}
```

---

### 10. Forgot Password
**POST** `/api/auth/forgot-password`

**Request Body:**
```json
{
  "email": "admin@auca.ac.rw"
}
```

**Response:**
```json
{
  "message": "Password reset request submitted successfully. It is now pending admin approval."
}
```

---

### 11. Reset Password
**POST** `/api/auth/reset-password`

**Request Body:**
```json
{
  "token": "your-reset-token-here",
  "password": "NewPassword123!"
}
```

**Response:**
```json
{
  "message": "Password has been reset successfully."
}
```

---

## Testing Flow

### Complete Authentication Flow:

1. **Check Health**: `GET /api/test/health`
2. **Check Database**: `GET /api/test/database`
3. **Check Admin User**: `GET /api/test/admin`
4. **Login as Admin**: `POST /api/auth/login` with admin credentials
5. **Check Console**: Look for OTP code in console output
6. **Verify OTP**: `POST /api/auth/verify-otp` with the OTP
7. **Get JWT Token**: Save the token from response
8. **Use Token**: Add `Authorization: Bearer <token>` header for protected endpoints

### Register New User Flow:

1. **Register**: `POST /api/auth/register` with user details
2. **Wait for Admin Approval**: (In Phase 4, admin can approve)
3. **Login**: `POST /api/auth/login` after approval
4. **Verify OTP**: `POST /api/auth/verify-otp`
5. **Access System**: Use JWT token

---

## Testing Tools

### Option 1: Postman
1. Download Postman: https://www.postman.com/downloads/
2. Import the endpoints above
3. Test each endpoint

### Option 2: cURL (Command Line)
```bash
# Health Check
curl http://localhost:5000/api/test/health

# Check Database
curl http://localhost:5000/api/test/database

# Check Admin
curl http://localhost:5000/api/test/admin

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@auca.ac.rw","password":"Admin123!"}'
```

### Option 3: Browser (GET requests only)
- Open browser and navigate to:
  - http://localhost:5000/api/test/health
  - http://localhost:5000/api/test/database
  - http://localhost:5000/api/test/admin

---

## Expected Console Output

When you run the application, you should see:
```
✅ Admin user created successfully!
   Email: admin@auca.ac.rw
   Password: Admin123!
```

When you login, you should see:
```
==================================================
📧 OTP EMAIL
==================================================
To: admin@auca.ac.rw
OTP Code: 123456
Expires: 5 minutes
==================================================
```

---

## Common Issues

### Issue 1: Database Connection Error
**Solution:** Make sure PostgreSQL is running and connection string in `appsettings.json` is correct

### Issue 2: Admin User Not Found
**Solution:** Run `dotnet ef database update` to ensure database is created

### Issue 3: OTP Not Received
**Solution:** Check console output for OTP code (email sending might fail if SMTP not configured)

### Issue 4: Invalid Credentials
**Solution:** Make sure you're using the correct password: `Admin123!`

---

## Next Steps

After testing these endpoints successfully:
- Phase 4: Implement Admin, Lecturer, Room services
- Phase 5: Create Razor Pages UI
- Phase 6: Testing & Deployment
