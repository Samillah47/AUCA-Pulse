# Quick Start Guide - Testing AUCA Pulse API

## Prerequisites
1. ✅ PostgreSQL installed and running
2. ✅ .NET 8.0 SDK installed
3. ✅ Postman installed (download from https://www.postman.com/downloads/)

## Step 1: Update Configuration

Edit `appsettings.json` and update:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=auca_pulse_db;Username=postgres;Password=YOUR_PASSWORD"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-minimum-32-characters-long-for-security",
    "Issuer": "AUCAPulse",
    "Audience": "AUCAPulseUsers",
    "ExpirationMinutes": 1440
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password",
    "SenderName": "AUCA Pulse"
  }
}
```

**Note:** Email settings are optional for testing. OTP will be displayed in console.

## Step 2: Run the Application

Open terminal in the project folder and run:

```bash
cd AUCAPulse
dotnet run
```

You should see:
```
✅ Admin user created successfully!
   Email: admin@auca.ac.rw
   Password: Admin123!

info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

**Keep this terminal window open!**

## Step 3: Import Postman Collection

1. Open Postman
2. Click **Import** button (top left)
3. Select **File** tab
4. Choose these two files:
   - `AUCA_Pulse_API.postman_collection.json`
   - `AUCA_Pulse_Local.postman_environment.json`
5. Click **Import**

## Step 4: Select Environment

1. In Postman, look at the top right corner
2. Click the dropdown that says "No Environment"
3. Select **AUCA Pulse - Local**

## Step 5: Test the API

### Test 1: Health Check
1. In Postman, expand **Test Endpoints** folder
2. Click **Health Check**
3. Click **Send**
4. You should see:
```json
{
  "status": "healthy",
  "timestamp": "2026-04-04T...",
  "message": "AUCA Pulse API is running"
}
```

### Test 2: Check Database
1. Click **Check Database**
2. Click **Send**
3. You should see:
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

### Test 3: Check Admin User
1. Click **Check Admin User**
2. Click **Send**
3. You should see admin user details

### Test 4: Login Flow (IMPORTANT!)

#### Step 4a: Login
1. Expand **Authentication** folder
2. Click **Login - Admin**
3. Click **Send**
4. You should see:
```json
{
  "status": "PENDING_OTP",
  "message": "OTP sent to email"
}
```

#### Step 4b: Get OTP from Console
5. **Go back to your terminal window** where the app is running
6. You should see something like:
```
==================================================
📧 OTP EMAIL
==================================================
To: admin@auca.ac.rw
OTP Code: 123456
Expires: 5 minutes
==================================================
```
7. **Copy the 6-digit OTP code** (e.g., 123456)

#### Step 4c: Verify OTP
8. In Postman, click **Verify OTP**
9. In the request body, replace `"123456"` with your actual OTP
10. Click **Send**
11. You should see:
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

**The JWT token is automatically saved to the environment variable!**

### Test 5: Register New User
1. Click **Register Student**
2. Click **Send**
3. You should see:
```json
{
  "message": "Registration submitted. Awaiting admin approval.",
  "userId": 2
}
```

## Common Testing Scenarios

### Scenario 1: Complete Login Flow
1. **Login - Admin** → Get PENDING_OTP status
2. Check console for OTP
3. **Verify OTP** → Get JWT token
4. Token is saved automatically

### Scenario 2: Register Different User Types
- **Register Student** → Creates student account
- **Register Lecturer** → Creates lecturer account
- **Register Staff** → Creates staff account

All new users will have status "PENDING" and need admin approval.

### Scenario 3: Password Reset Flow
1. **Forgot Password** → Creates reset request
2. Admin approves (Phase 4)
3. **Reset Password** → Changes password

## Troubleshooting

### Issue: "Connection refused" or "Cannot connect"
**Solution:** Make sure the application is running (`dotnet run`)

### Issue: "Database connection error"
**Solution:** 
- Check PostgreSQL is running
- Verify connection string in `appsettings.json`
- Run `dotnet ef database update`

### Issue: "Admin user not found"
**Solution:** 
- Stop the application (Ctrl+C)
- Run `dotnet ef database drop --force`
- Run `dotnet ef database update`
- Run `dotnet run` again

### Issue: "Invalid OTP"
**Solution:** 
- OTP expires in 5 minutes
- Make sure you're using the latest OTP from console
- Login again to get a new OTP

### Issue: "Invalid credentials"
**Solution:** 
- Admin email: `admin@auca.ac.rw`
- Admin password: `Admin123!` (case-sensitive)

## What's Working Now

✅ Database with all tables and seed data
✅ Admin user auto-creation
✅ User registration (Student, Lecturer, Staff)
✅ Login with password verification
✅ OTP generation and validation
✅ JWT token generation
✅ Password reset request
✅ Email service (console logging)

## What's Coming Next

📋 Phase 4: Admin approval workflows, Lecturer services, Room management
📋 Phase 5: Razor Pages UI (Web interface)
📋 Phase 6: Testing & Deployment

## Need Help?

Check the detailed documentation:
- `API_TESTING_GUIDE.md` - Complete API documentation
- `README.md` - Project overview

## Testing Checklist

- [ ] Health check works
- [ ] Database connection works
- [ ] Admin user exists
- [ ] Can see all roles (4 roles)
- [ ] Can see all locations (8 locations)
- [ ] Can see all rooms (6 rooms)
- [ ] Can login as admin
- [ ] Can see OTP in console
- [ ] Can verify OTP and get JWT token
- [ ] Can register new student
- [ ] Can register new lecturer
- [ ] Can register new staff

Once all checkboxes are ✅, you're ready for Phase 4!
