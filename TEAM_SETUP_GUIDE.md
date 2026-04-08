# Team Setup Guide - AUCA Pulse

## Problem: Styles Not Working After Pulling Code

If you pulled the code and the website has no styles (looks broken), it's because **client-side libraries (Bootstrap, jQuery) are not committed to Git**.

## Solution: Restore Client-Side Libraries

### Option 1: Using LibMan CLI (Recommended)

1. **Install LibMan CLI globally** (one-time setup):
   ```bash
   dotnet tool install -g Microsoft.Web.LibraryManager.Cli
   ```

2. **Navigate to project folder**:
   ```bash
   cd "c:\path\to\FinalProject_GroupB"
   ```

3. **Restore libraries**:
   ```bash
   libman restore
   ```

4. **Verify** - Check that `wwwroot/lib/` now contains:
   - `bootstrap/dist/` (CSS and JS files)
   - `jquery/dist/` (jQuery files)
   - `jquery-validation/dist/`
   - `jquery-validation-unobtrusive/`

---

### Option 2: Using Visual Studio

1. **Open** `FinalProject_GroupB.sln` in Visual Studio

2. **Right-click** on the project in Solution Explorer

3. **Select** "Manage Client-Side Libraries"

4. **Click** "Restore" button (or it will restore automatically)

---

### Option 3: Manual Download (Last Resort)

If LibMan doesn't work, manually download and extract:

#### Bootstrap 5.3.0
1. Download: https://github.com/twbs/bootstrap/releases/download/v5.3.0/bootstrap-5.3.0-dist.zip
2. Extract to: `wwwroot/lib/bootstrap/`

#### jQuery 3.7.1
1. Download: https://code.jquery.com/jquery-3.7.1.min.js
2. Save to: `wwwroot/lib/jquery/dist/jquery.min.js`

#### jQuery Validation 1.19.5
1. Download: https://cdn.jsdelivr.net/npm/jquery-validation@1.19.5/dist/jquery.validate.min.js
2. Save to: `wwwroot/lib/jquery-validation/dist/jquery.validate.min.js`

#### jQuery Validation Unobtrusive 4.0.0
1. Download: https://cdn.jsdelivr.net/npm/jquery-validation-unobtrusive@4.0.0/dist/jquery.validate.unobtrusive.min.js
2. Save to: `wwwroot/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js`

---

## Complete Setup Checklist

After pulling the code, follow these steps:

### 1. Restore NuGet Packages
```bash
dotnet restore
```

### 2. Restore Client-Side Libraries
```bash
libman restore
```

### 3. Update Database
```bash
dotnet ef database update
```

### 4. Configure Settings
Edit `appsettings.json`:
- Update PostgreSQL connection string
- Update email settings (SMTP)
- Update JWT secret key

### 5. Run Application
```bash
dotnet run
```
Or press **F5** in Visual Studio.

### 6. Access Application
Open browser: http://localhost:5204

---

## Troubleshooting

### Issue: "libman: command not found"
**Solution:** Install LibMan CLI:
```bash
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
```

### Issue: "Database does not exist"
**Solution:** Run migrations:
```bash
dotnet ef database update
```

### Issue: "Styles still not loading"
**Solution:** 
1. Clear browser cache (Ctrl+Shift+Delete)
2. Hard refresh (Ctrl+F5)
3. Check browser console for errors (F12)
4. Verify files exist in `wwwroot/lib/bootstrap/dist/css/`

### Issue: "Cannot connect to database"
**Solution:** 
1. Ensure PostgreSQL is running
2. Check connection string in `appsettings.json`
3. Verify database name: `auca_pulse_db`

---

## Why Are Libraries Not in Git?

Client-side libraries (Bootstrap, jQuery) are **excluded from Git** because:
- ✅ They are large files (10+ MB)
- ✅ They can be downloaded from CDN
- ✅ They don't change with our code
- ✅ Keeps repository size small

**Best Practice:** Use LibMan to manage them, just like NuGet manages server-side packages.

---

## File Structure After Setup

```
FinalProject_GroupB/
├── wwwroot/
│   ├── lib/
│   │   ├── bootstrap/
│   │   │   └── dist/
│   │   │       ├── css/
│   │   │       │   ├── bootstrap.css
│   │   │       │   ├── bootstrap.min.css
│   │   │       │   └── ...
│   │   │       └── js/
│   │   │           ├── bootstrap.js
│   │   │           ├── bootstrap.min.js
│   │   │           └── ...
│   │   ├── jquery/
│   │   │   └── dist/
│   │   │       ├── jquery.js
│   │   │       └── jquery.min.js
│   │   ├── jquery-validation/
│   │   │   └── dist/
│   │   └── jquery-validation-unobtrusive/
│   ├── css/
│   │   └── site.css
│   └── js/
│       └── site.js
├── Controllers/
├── Models/
├── Services/
└── ...
```

---

## Quick Commands Reference

```bash
# Install LibMan (one-time)
dotnet tool install -g Microsoft.Web.LibraryManager.Cli

# Restore everything
dotnet restore          # NuGet packages
libman restore          # Client-side libraries
dotnet ef database update  # Database

# Run application
dotnet run

# Build application
dotnet build

# Clean build artifacts
dotnet clean
```

---

## Need Help?

Contact: **Habiyaremye Adolphe** (26751)
Branch: `Habiyaremye_Adolphe_26751`
