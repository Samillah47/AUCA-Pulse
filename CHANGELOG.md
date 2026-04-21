# Changelog

User-visible changes for the AUCA Pulse campus portal, grouped by feature area.
All changes land on branch `Habiyaremye_Adolphe_26751`.

---

## Course assignment (Section 1)

- Admins can now assign a single course to **many lecturers in one submit** using
  the redesigned Assign Courses page. The form offers a searchable checkbox list
  with Select-all / Clear / live-filter controls.
- A new `POST /api/courseassignments/bulk` endpoint accepts an array of lecturer
  IDs. Duplicates (already-assigned lecturers) are skipped gracefully with a
  friendly note; the request never returns a 500 for that reason.
- The current-assignments list is now grouped by course and shows the roster of
  assigned lecturers per course at a glance.

## Timetable (Section 2)

- The timetable page now **persists across navigation and browser restarts**. On
  every visit it reloads the saved `LectureSchedule` rows for the selected
  semester from the database (the data was always saved; only the UI lost it).
- New **semester** and **lecturer** filter dropdowns at the top. The semester
  defaults to the active one; changing the lecturer filters the grid to just
  their classes.
- New **Print / PDF** button with a print stylesheet that hides the sidebar and
  chrome for clean print-outs.
- The generator form is collapsed by default; regenerating with "Replace
  existing" ticked prompts a confirm dialog.

## Notifications and error messages (Section 3)

- A lightweight **toast** system has been added (`wwwroot/js/toast.js`). Any page
  that sets `TempData["SuccessMessage"]`, `ErrorMessage`, `InfoMessage`, or
  `WarningMessage` now surfaces it as an animated top-right toast that
  auto-dismisses after 5 seconds (up to 3 stacked).
- A **global exception middleware** catches unhandled errors, logs the full stack
  trace server-side, and returns a plain-English message:
    - `/api/*` routes get a JSON `{ message: "..." }` body.
    - UI routes redirect to a friendly `/Error` page.
  Well-known exceptions (PostgreSQL unique violation / FK violation / not-null,
  `DbUpdateException`, `KeyNotFoundException`, `UnauthorizedAccessException`,
  `TimeoutException`) are translated into human sentences before display.
- The login error `"Invalid credentials"` has been replaced with
  `"The email or password you entered doesn't match our records."` and
  PENDING / REJECTED accounts now give distinct, helpful messages instead of
  the enum name.
- The default `/Error` page is redesigned — friendly card with a "Back to
  Dashboard" button. No stack traces or SQL codes are ever shown to the user.

## Password reset (Section 4)

- The **admin-approval gate is removed**. When a user submits a reset request
  the system auto-approves it and emails the reset link directly.
- Tokens are **stored hashed** (SHA-256) in the database; only the raw token is
  ever in the email.
- Reset links **expire in 30 minutes** (down from 1 hour) and can only be used
  **once**. Requesting a new link invalidates any pending tokens for the user.
- The Forgot Password page **does not leak** whether an email is registered —
  the success message is the same either way.
- Two new user-facing pages:
    - `/ForgotPassword` — single email field, confirmation message after submit.
    - `/ResetPassword?token=...` — new password + confirm-password with
      real-time match validation, redirects to the login page on success.
- Email template updated: clearer subject line, 30-minute expiry copy, and a URL
  built from `ApiSettings:BaseUrl` so it works in any environment.

## Visual design (Section 6)

- Complete repaint to a **modern, mostly-white** theme with a single accent
  blue (`#2563eb`). All purple gradients are gone.
- New design-token palette in `wwwroot/css/site.css` (CSS variables for
  surfaces, borders, text, accent, semantic colours, shadows, radius, and an
  8-pixel spacing grid).
- **Inter** typography (loaded from Google Fonts) at 15-16 px base, 600-weight
  headings, 1.5 line-height.
- **Sidebar** is now white with a 1-pixel right border; muted text by default,
  active link rendered in accent blue on an accent-soft background.
- **Topbar** is white with a single 1-pixel bottom border, no box-shadow.
- **Cards** get soft borders + a small shadow that grows on hover.
- **Buttons**: accent-blue primary, outlined secondary, tertiary text-only.
  Bootstrap's default blue / purple is overridden everywhere.
- **Tables** have no zebra stripes — row hover with a 1-pixel bottom border.
- **Badges** use softer, branded variants instead of solid Bootstrap blocks.
- **Empty states** have a reusable helper with an icon, heading, and sub-line.
- The ⚡ emoji brand mark is replaced with an inline SVG.
- All inline `<style>` is removed from `_Layout.cshtml`.

## Student portal (Section 7)

- A new `_StudentLayout.cshtml` — **no sidebar, top nav only**, centred at
  max-width 1200 px, with a footer. Students never see the admin sidebar again.
- New `/Home` student landing page with:
    - Hero greeting using the student's first name + today's date
    - Today's class summary (count)
    - Four quick tiles: Find a lecturer, My timetable, Free rooms, Notifications
    - Horizontal scroll of today's classes (current semester, filtered by
      current day of week)
    - Latest 5 announcements
    - Friendly empty states throughout
- After OTP verification, **students are redirected to `/Home`**; everyone else
  still lands on `/Dashboard`. Students who navigate to `/Dashboard` are
  redirected away.

## Student timetable (Section 8)

- A new `/Student/Timetable` page renders the weekly grid (Monday-Friday × time
  slots) for the current semester, using the same persisted `LectureSchedule`
  data as the admin timetable page.

---

## Known follow-ups (not in this changelog)

- A dedicated `Enrollment` model to filter the student timetable to the student's
  own courses only. Currently the student timetable shows the full semester grid.
- Announcement audience / category extensions on the `Notification` model.
- Role-specific dashboard enhancements (activity feed, bulk CSV import, lecturer
  quick-status buttons, staff directory).
- SignalR for live status / room updates.
- Dark-mode toggle using `[data-theme="dark"]` variable overrides.

---

## Existing features unchanged

- Round-Robin timetable generator (`Services/TimetableGeneratorService.cs`)
- Round-Robin room auto-assign Singleton (`Services/RoundRobinRoomService.cs`)
- Background room auto-release service (`Services/RoomAutoReleaseService.cs`)
- JWT + OTP authentication, role-based authorization
- Admin pages: User management, Verification requests, Courses, Reports
