# Why Styles Don't Work After Pulling Code - SOLVED

## The Problem

When your team members pull your code from Git and run the application, they see:
- ❌ No Bootstrap styles (plain HTML)
- ❌ Broken layout
- ❌ No colors, buttons, or formatting
- ❌ Website looks like it's from 1995

## Root Cause

The `wwwroot/lib/` folder containing Bootstrap, jQuery, and other client-side libraries is **excluded from Git** by `.gitignore`:

```gitignore
# Line 127 in .gitignore
wwwroot/lib/
```

**Why?**
- Client-side libraries are large files (10+ MB)
- They don't change with your code
- They can be downloaded from CDN
- Best practice: Don't commit dependencies to Git

## The Solution

Use **LibMan (Library Manager)** to restore client-side libraries, just like NuGet restores server-side packages.

### For Your Team Members

**Step 1: Install LibMan CLI** (one-time setup)
```bash
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
```

**Step 2: Navigate to project folder**
```bash
cd "path/to/FinalProject_GroupB"
```

**Step 3: Restore libraries**
```bash
libman restore
```

**Step 4: Run application**
```bash
dotnet run
```

**Done!** Styles will now work perfectly.

---

## What LibMan Does

The `libman.json` file tells LibMan what to download:

```json
{
  "version": "1.0",
  "defaultProvider": "cdnjs",
  "libraries": [
    {
      "library": "bootstrap@5.3.0",
      "destination": "wwwroot/lib/bootstrap/dist/",
      "files": ["css/bootstrap.min.css", "js/bootstrap.bundle.min.js", ...]
    },
    {
      "library": "jquery@3.7.1",
      "destination": "wwwroot/lib/jquery/dist/",
      "files": ["jquery.min.js", ...]
    }
  ]
}
```

When you run `libman restore`, it:
1. Downloads Bootstrap 5.3.0 from CDNJS
2. Downloads jQuery 3.7.1 from CDNJS
3. Downloads jQuery Validation libraries
4. Saves them to `wwwroot/lib/` folders

---

## Files Downloaded

After `libman restore`, you'll have:

```
wwwroot/lib/
├── bootstrap/
│   └── dist/
│       ├── css/
│       │   ├── bootstrap.css
│       │   ├── bootstrap.min.css
│       │   └── ... (10 CSS files)
│       └── js/
│           ├── bootstrap.js
│           ├── bootstrap.bundle.min.js
│           └── ... (8 JS files)
├── jquery/
│   └── dist/
│       ├── jquery.js
│       ├── jquery.min.js
│       └── jquery.min.map
├── jquery-validation/
│   └── dist/
│       ├── jquery.validate.js
│       ├── jquery.validate.min.js
│       └── additional-methods.min.js
└── jquery-validation-unobtrusive/
    ├── jquery.validate.unobtrusive.js
    └── jquery.validate.unobtrusive.min.js
```

**Total size:** ~2.5 MB

---

## How Layout References These Files

In `Pages/Shared/_Layout.cshtml`:

```html
<head>
    <!-- Bootstrap CSS -->
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <!-- Your content -->
    
    <!-- jQuery -->
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <!-- Bootstrap JS -->
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <!-- Validation -->
    <script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
    <script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"></script>
</body>
```

If these files don't exist, the browser shows 404 errors and styles don't load.

---

## Alternative: Use CDN (Not Recommended for Development)

Instead of local files, you could use CDN links:

```html
<!-- From CDN instead of local -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
```

**Drawbacks:**
- ❌ Requires internet connection
- ❌ Slower (external request)
- ❌ CDN could go down
- ❌ Version changes could break your app

**Local files are better for development.**

---

## Comparison: NuGet vs LibMan

| Aspect | NuGet | LibMan |
|--------|-------|--------|
| **Purpose** | Server-side packages | Client-side libraries |
| **Examples** | EF Core, JWT, BCrypt | Bootstrap, jQuery |
| **Location** | Compiled into DLL | wwwroot/lib/ |
| **Restore Command** | `dotnet restore` | `libman restore` |
| **Config File** | .csproj | libman.json |
| **In Git?** | No (packages/) | No (wwwroot/lib/) |

Both are excluded from Git and must be restored after cloning.

---

## Troubleshooting

### Issue: "libman: command not found"
```bash
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
```

### Issue: "Styles still not working"
1. Check browser console (F12) for 404 errors
2. Verify files exist: `dir wwwroot\lib\bootstrap\dist\css`
3. Clear browser cache (Ctrl+Shift+Delete)
4. Hard refresh (Ctrl+F5)

### Issue: "LibMan restore fails"
Check `libman.json` syntax and file paths match CDNJS structure.

---

## Summary

**Problem:** Client-side libraries not in Git  
**Solution:** Run `libman restore` after pulling code  
**Why:** Best practice to keep repository small and manageable  
**Analogy:** Same as running `dotnet restore` for NuGet packages  

**Tell your team:** "After pulling, run `libman restore` before `dotnet run`"

---

## Complete Setup Workflow for Team

```bash
# 1. Clone repository
git clone https://github.com/Samillah47/AUCA-Pulse.git
cd "AUCA-Pulse/FinalProject_GroupB"
git checkout Habiyaremye_Adolphe_26751

# 2. Install LibMan (one-time)
dotnet tool install -g Microsoft.Web.LibraryManager.Cli

# 3. Restore everything
dotnet restore          # Server-side packages
libman restore          # Client-side libraries

# 4. Update database
dotnet ef database update

# 5. Run application
dotnet run
```

**Now styles will work! ✅**
