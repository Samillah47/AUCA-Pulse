# Deploying AUCA Pulse to Render (free tier)

This project is configured to deploy as a Docker web service on
[Render](https://render.com) with a free PostgreSQL database, using the
[`render.yaml`](render.yaml) blueprint at the repo root.

## What you get on the free plan

- **1 web service** — sleeps after 15 minutes of inactivity, ~30s cold start.
- **1 PostgreSQL database** — 256 MB, expires 90 days after creation.
  Re-create it before the deadline (or upgrade to Starter $7/mo) so
  you don't lose data.
- **HTTPS** with an auto-issued certificate, custom subdomain
  `your-service.onrender.com`.

That's enough for a class demo. For a production rollout you'd move to
Render's Starter plan (or a Linux VM) so the service doesn't sleep.

## One-time setup

### 1. Push the deploy artefacts

This branch already contains:

- `Dockerfile` — multi-stage build, restores client libs via LibMan,
  publishes a Release build.
- `.dockerignore` — keeps the build context lean.
- `render.yaml` — defines the web service + Postgres + env vars.
- `Program.cs` — translates Render's `DATABASE_URL` into an Npgsql
  connection string, auto-applies EF migrations on production startup,
  trusts `X-Forwarded-*` headers from Render's load balancer.

Make sure all of these are pushed to the branch you want Render to
build (e.g. `final/team-merge`).

### 2. Create a Render account and connect GitHub

1. Sign up at <https://render.com> with the GitHub account that owns
   the repo (or a teammate's).
2. Authorise Render to read **AUCA-Pulse**.

### 3. Create from the blueprint

1. From the Render dashboard, click **New ▾ → Blueprint**.
2. Pick the **AUCA-Pulse** repo and the branch
   (`final/team-merge` is the current integration branch).
3. Render reads `render.yaml` and proposes:
   - a **PostgreSQL database** named `auca-pulse-db`, plan: free.
   - a **Docker web service** named `auca-pulse`, plan: free.
4. Click **Apply**. The first build takes ~5–10 minutes
   (`dotnet publish` is the slow step).

### 4. Set the SMTP secrets

`render.yaml` declares two `sync: false` env vars on the web service —
Render asks you for them on first deploy and never commits them to git:

| Variable | What to put |
|---|---|
| `EmailSettings__SenderEmail` | The Gmail address you send OTP/reset emails from |
| `EmailSettings__SenderPassword` | A Gmail **app password** (not the account password) |

Generate the app password at <https://myaccount.google.com/apppasswords>
(2FA must be enabled on the account). Without these, signup OTP and
password-reset email flows will throw and the user will see a friendly
"could not send email" message.

### 5. Wait for the first deploy

Render shows a live build log. Once it says **Live**, open the URL
(something like `https://auca-pulse.onrender.com`).

The first request after a deploy or after 15 min idle takes ~30 seconds
while the container wakes up — that's the free tier.

## Logging in for the first time

Migrations are applied automatically on startup. The seed data in
`Helpers/AdminUserInitializer.cs` provisions an admin user the first
time the database is empty. Use those credentials to log in, then:

1. Verify any pending lecturer / staff signups from
   **Verification Requests**.
2. Create the **Semester**, **Courses**, **Groups**, and
   **Course Assignments**.
3. Hit **Generate Timetable** — Round Robin builds the weekly schedule.

## Updating the deployment

`autoDeploy: true` is set in `render.yaml`, so every push to the
configured branch triggers a fresh build and rollout. Watch the logs
under the service's **Events** tab.

For a manual deploy, click **Manual Deploy ▾ → Deploy latest commit**
on the service page.

## Troubleshooting

### "Application failed to respond"
Cold start. Wait ~30 seconds and retry. If it persists, check the
service logs for a startup exception (most often a malformed
`DATABASE_URL` or a missing migration).

### Postgres connection failures
Render's free Postgres requires SSL — the connection-string converter
in `Program.cs` adds `SSL Mode=Require;Trust Server Certificate=true`
automatically. If you ever set `ConnectionStrings__DefaultConnection`
manually, keep both flags or the connection will fail with
*"server does not support SSL"*.

### "Failed to send email"
SMTP creds aren't set, or you used a real Gmail password instead of an
app password. Re-check `EmailSettings__SenderEmail` and
`EmailSettings__SenderPassword` in the service **Environment** tab.

### My data disappeared
The free Postgres tier expires after 90 days. Render warns you by
email; create a new database, update the `DATABASE_URL` env var (or
re-link via the blueprint), and migrations will populate it on next
boot.

## Running the same Docker image locally

```bash
docker build -t auca-pulse .
docker run --rm -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e DATABASE_URL="postgres://user:pass@host:5432/auca_pulse_db" \
  -e JwtSettings__SecretKey="local-dev-secret-at-least-32-chars-long" \
  -e EmailSettings__SmtpServer=smtp.gmail.com \
  -e EmailSettings__SmtpPort=587 \
  -e EmailSettings__SenderEmail=you@gmail.com \
  -e EmailSettings__SenderPassword=your-app-pass \
  auca-pulse
```

Then open <http://localhost:8080>.
