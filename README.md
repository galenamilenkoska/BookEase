# BookEase – Barbershop Appointment Booking App

A full-featured, portfolio-quality appointment booking web app for a barbershop. Built with .NET 9, Blazor, and Entity Framework Core. Clean responsive UI, real-time slot availability, and a complete admin dashboard.

---

## Screenshots

> Run `dotnet run` and visit the URLs below to see the live app.

| Page | Route | Description |
|---|---|---|
| Landing | `/` | Hero, service cards with prices, "why us" section, CTA |
| Book | `/book` | 3-step wizard: service → date/time slots → contact form |
| Confirmation | `/confirmation?appointmentId=N` | Booking summary with status badge |
| Admin | `/admin` | Filterable appointments table with confirm/cancel actions |

**Color scheme:** Deep navy (`#1a1a2e`) + warm gold (`#c9a84c`) + off-white background — styled to feel like a premium grooming brand.

---

## Features

- **Public landing page** with hero section, service cards with pricing/duration, "why us" section, and call-to-action
- **3-step booking wizard**: pick a service → choose date + available time slot → enter contact details
- **Smart slot availability**: 30-minute slots 9AM–5PM only, past slots hidden, respects each service's duration when computing end-time
- **Double-booking prevention**: availability recalculated server-side immediately before saving — race conditions handled
- **Confirmation page** with full appointment summary
- **Admin dashboard** at `/admin`: filter by status, confirm or cancel each appointment in one click
- **Seed data**: 4 realistic services pre-loaded on first run, database created automatically

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 9 — Blazor Web App |
| Rendering | Interactive Server (SignalR-based, no page reloads) |
| Database | SQLite via Entity Framework Core 9 |
| ORM pattern | `IDbContextFactory<T>` — safe for Blazor Server circuits |
| Styling | Custom CSS — Playfair Display + Inter (Google Fonts) |
| Validation | `DataAnnotationsValidator` — client + server both |

---

## Running Locally

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

```bash
# Clone / navigate to project directory
cd BookEase

# Run — database and seed data created automatically on first start
dotnet run
```

Then open:
- **HTTP:** `http://localhost:5162`
- **HTTPS:** `https://localhost:7229`

The SQLite database file (`bookease.db`) is created in the project root on first run, pre-seeded with 4 barbershop services.

---

## Deployment

### Option A — Render (Free Tier, recommended)

Render supports ASP.NET Core apps via Docker. Add a `Dockerfile` to the project root:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookEase.dll"]
```

Then on [render.com](https://render.com):
1. **New → Web Service** → connect your GitHub repo
2. **Environment:** Docker
3. **Add environment variable:** `ASPNETCORE_URLS=http://+:8080`
4. **Disk** (optional): mount `/app` as a persistent disk so `bookease.db` survives redeploys
5. Click **Deploy** — free tier spins down after inactivity, wakes on next request

> **Note:** For production swap SQLite for PostgreSQL (free on Render) and swap `UseSqlite` for `UseNpgsql` in `Program.cs`.

---

### Option B — Azure App Service (Free F1 Tier)

```bash
# Install Azure CLI if needed: https://aka.ms/installazurecliwindows

# Login
az login

# Create resource group and App Service plan (free tier)
az group create --name bookease-rg --location eastus
az appservice plan create --name bookease-plan --resource-group bookease-rg --sku FREE

# Create the web app (.NET 9)
az webapp create \
  --resource-group bookease-rg \
  --plan bookease-plan \
  --name bookease-YOUR-UNIQUE-NAME \
  --runtime "DOTNETCORE:9.0"

# Publish and deploy
dotnet publish -c Release -o ./publish
cd publish
zip -r ../deploy.zip .
az webapp deploy \
  --resource-group bookease-rg \
  --name bookease-YOUR-UNIQUE-NAME \
  --src-path ../deploy.zip

# Your app is live at:
# https://bookease-YOUR-UNIQUE-NAME.azurewebsites.net
```

> **SQLite on Azure:** The App Service file system is ephemeral on the free tier. For persistence, either upgrade to a paid plan with a mounted storage share, or swap to Azure SQL / PostgreSQL.

---

### Option C — Self-Host / VPS (Ubuntu)

```bash
# On your server
dotnet publish -c Release -o /var/www/bookease

# Create systemd service
sudo nano /etc/systemd/system/bookease.service
```

```ini
[Unit]
Description=BookEase Blazor App
After=network.target

[Service]
WorkingDirectory=/var/www/bookease
ExecStart=/usr/bin/dotnet BookEase.dll
Restart=always
RestartSec=10
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5162

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl enable bookease
sudo systemctl start bookease
# Reverse-proxy with nginx pointing to localhost:5162
```

---

## Project Structure

```
BookEase/
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor       # Sticky header, footer, mobile nav
│   └── Pages/
│       ├── Home.razor             # Landing page (static render)
│       ├── Book.razor             # 3-step booking wizard (interactive server)
│       ├── Confirmation.razor     # Post-booking summary
│       └── Admin.razor            # Admin dashboard (interactive server)
├── Data/
│   └── AppDbContext.cs            # EF Core DbContext + HasData seed
├── Models/
│   ├── Service.cs                 # Service entity
│   └── Appointment.cs             # Appointment entity + AppointmentStatus enum
├── wwwroot/
│   └── app.css                    # All custom styles (~700 lines)
├── Program.cs                     # DI setup, EnsureCreated on startup
└── bookease.db                    # SQLite file (auto-created, git-ignored)
```

---

## Admin Dashboard

Visit `/admin` to manage appointments. Features:

- **Filter tabs**: Upcoming / All / Pending / Confirmed / Cancelled
- **Confirm** button — marks appointment as Confirmed (green badge)
- **Cancel** button — marks appointment as Cancelled (red badge)
- Summary row at the bottom with counts per status

No authentication is included — add ASP.NET Core Identity or a simple password middleware before deploying publicly.

---

## Extending the App

Some ideas for taking this further:
- **Auth**: Add ASP.NET Core Identity to protect `/admin`
- **Email confirmations**: Use SendGrid or Mailkit to email customers on booking
- **iCal / Google Calendar**: Export appointments as `.ics` files
- **PostgreSQL**: Swap SQLite for a production database on any cloud host
- **Time zones**: Store `StartTime` as UTC, display in the customer's local time
