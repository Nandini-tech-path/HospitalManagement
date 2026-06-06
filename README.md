# Hospital Management System on Azure

A complete **Hospital Management System** built with ASP.NET Core MVC (.NET 8), Entity Framework Core, SQL Server, and ASP.NET Core Identity. Features role-based dashboards, patient/doctor/appointment/billing management, analytics with Chart.js, and Azure deployment readiness.

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue) ![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-purple) ![Bootstrap 5](https://img.shields.io/badge/Bootstrap-5-7952b3)

---

## Features

| Module | Capabilities |
|--------|-------------|
| **Authentication** | Login, logout, password hashing, role-based authorization |
| **Patients** | Add, edit, delete, view, search, pagination, sorting |
| **Doctors** | Full CRUD with search and pagination (Admin) |
| **Appointments** | Schedule, edit, cancel, complete, search |
| **Billing** | Generate bills, view, print, payment tracking |
| **Reports** | Patient, doctor, appointment, and revenue charts |
| **Dashboards** | Role-specific admin, doctor, and receptionist views |

### User Roles

| Role | Permissions |
|------|-------------|
| **Admin** | Full access: patients, doctors, appointments, billing, reports, dashboard |
| **Doctor** | View assigned patients, today's/upcoming appointments, update status |
| **Receptionist** | Register patients, schedule appointments, manage billing |

---

## Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@hospital.com` | `Admin@123` |
| Doctor | `doctor@hospital.com` | `Doctor@123` |
| Receptionist | `reception@hospital.com` | `Reception@123` |

### Seeded Sample Data

- 10 Patients
- 5 Doctors
- 20 Appointments
- 10 Bills

---

## Local Setup

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) (or VS Code)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (included with Visual Studio)

### 1. Clone and Restore

```powershell
cd hospital-mng
dotnet restore
```

### 2. Database Configuration

**Default (SQL Server LocalDB)** — configured in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HospitalManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

**Optional SQLite fallback** (if LocalDB is not installed):

```powershell
dotnet run --project HospitalManagement --DatabaseProvider Sqlite --ConnectionStrings:DefaultConnection "Data Source=hospital.db"
```

### 3. Apply Migrations (SQL Server)

```powershell
cd HospitalManagement
dotnet ef database update
```

> Migrations run automatically on startup. Seed data is applied on first run.

### 4. Run the Application

```powershell
cd HospitalManagement
dotnet run
```

Open the URL shown in the console (typically `https://localhost:7xxx` or `http://localhost:5xxx`).

### 5. Build the Solution

```powershell
dotnet build HospitalManagement.sln
```

---

## Project Structure

```
hospital-mng/
├── HospitalManagement.sln
├── README.md
└── HospitalManagement/
    ├── Controllers/          # MVC controllers
    ├── Data/                 # DbContext, migrations, seed data
    ├── Models/               # Domain entities and view models
    ├── Services/             # Dashboard and report services
    ├── Views/                # Razor views with healthcare UI
    ├── wwwroot/css/site.css  # Custom healthcare theme
    ├── Migrations/           # EF Core SQL Server migrations
    ├── appsettings.json
    ├── appsettings.Development.json
    └── appsettings.Production.json
```

---

## Azure Deployment

This application is ready to be deployed to Azure using either a fully managed Platform-as-a-Service (PaaS) model or an Infrastructure-as-a-Service (IaaS) model.

### Option A: Azure App Service (Recommended - PaaS)

Azure App Service is the easiest and most robust way to host this .NET 8 web app.

#### 1. Create the App Service Resource
1. Sign in to the **Azure Portal**.
2. Click **Create a resource** and select **Web App**.
3. Configure the following:
   - **Runtime stack**: `.NET 8 (LTS)`
   - **Operating System**: `Linux` or `Windows` (Linux is generally cheaper and faster to start).
   - **Pricing Plan**: Basic/Standard (or Free `F1` for simple demo testing).

#### 2. Configure Environment Settings
1. In the Web App's left-hand sidebar, navigate to **Settings** → **Environment variables** (or **Configuration** in older portal views).
2. Under **Connection strings**, add:
   - **Name**: `DefaultConnection`
   - **Value**: `Server=tcp:YOUR_SQL_SERVER.database.windows.net,1433;Initial Catalog=HospitalManagementDb;Persist Security Info=False;User ID=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`
   - **Type**: `SQLServer`
3. Under **App settings** (Environment variables), add:
   - **Name**: `ASPNETCORE_ENVIRONMENT`
   - **Value**: `Production`

#### 3. Publish the Code
You can deploy using your preferred tool:

* **From Visual Studio 2022**:
  1. Right-click the `HospitalManagement` project → **Publish**.
  2. Select **Target** as **Azure** → **Azure App Service (Windows/Linux)**.
  3. Select your subscription, resource group, and Web App name.
  4. Click **Publish** to deploy.
  
* **From GitHub Actions (CI/CD)**:
  1. Go to the Web App's **Deployment Center** and select **GitHub** as the source.
  2. Authorize and select your repository. Azure will automatically create a workflow file that builds and deploys your code on every push to the `main` branch.

---

### Option B: Azure Virtual Machine (IaaS)

If you require full OS control, you can host the app on a Windows Server VM using IIS.

#### 1. VM Configuration
1. Create a **Windows Server VM** in the Azure Portal.
2. Ensure you enable port **80** (HTTP) and **443** (HTTPS) in the Network Security Group (NSG) inbound rules.
3. RDP into the VM and install the **.NET 8 Hosting Bundle**: [Download link](https://dotnet.microsoft.com/download/dotnet/8.0).
4. Install and enable **IIS** (Internet Information Services) via Server Manager → Add Roles and Features.

#### 2. IIS Setup & Deployment
1. Right-click the project in Visual Studio → **Publish** → **Folder**.
2. Copy the published folder files to your VM (e.g. to `C:\inetpub\wwwroot\hospital`).
3. In IIS Manager, create a new Website pointing to that folder.
4. Set the Website's Application Pool to **No Managed Code**.
5. Add an Environment Variable for the app pool:
   - Key: `ASPNETCORE_ENVIRONMENT`, Value: `Production`.

---

### Azure SQL Database Setup

Both hosting options require an Azure SQL Database.

1. **Create an Azure SQL Database**:
   - Go to the Azure Portal → **SQL Databases** → **Create**.
   - Create a SQL Database and a new logical SQL Server.
   - Configure authentication (SQL authentication or Microsoft Entra ID).
2. **Configure Database Firewall**:
   - Go to your logical SQL Server page in Azure.
   - Under **Security** → **Firewalls and virtual networks**:
     - Toggle **Allow Azure services and resources to access this server** to **Yes** (required for App Service/VM connection).
     - Add your local IP to the client firewall rules if you want to run EF migrations from your machine.
3. **Run EF Database Migrations**:
   - The application automatically calls `context.Database.MigrateAsync()` in `DbInitializer` on startup when configured with SQL Server. It will build and seed the schema automatically on first run.
   - Alternatively, you can apply migrations manually from your local development terminal:
     ```powershell
     dotnet ef database update --project HospitalManagement --connection "Server=tcp:YOUR_SQL_SERVER.database.windows.net... [full connection string]"
     ```

---

### Azure Backup and Recovery

#### 1. SQL Database Backup
- Azure SQL databases feature **automated, built-in backups** by default.
- Backups are stored with geo-redundancy and support point-in-time restores (up to 7-35 days depending on service tier).

#### 2. VM Backup (If using Option B)
1. Set up a **Recovery Services Vault** in the Azure portal.
2. Select **Backup** and associate your Virtual Machine.
3. Define a backup schedule policy (e.g., daily incremental backup).

#### Disaster Recovery Overview

| Scenario | RPO | RTO | Procedure |
|----------|-----|-----|-----------|
| Database corruption | 5 min | 1 hour | Point-in-time restore from Azure SQL |
| VM failure | 24 hours | 2–4 hours | Restore VM from backup vault |
| Region outage | Varies | Varies | Geo-replicated SQL + secondary VM |

---

## Configuration Reference

| File | Purpose |
|------|---------|
| `appsettings.json` | Default LocalDB connection |
| `appsettings.Development.json` | Development logging |
| `appsettings.Production.json` | Azure SQL connection template |
| `DatabaseProvider` | `SqlServer` (default) or `Sqlite` |

---

## Technology Stack

- **Backend:** ASP.NET Core MVC (.NET 8), EF Core, ASP.NET Core Identity
- **Database:** SQL Server LocalDB / Azure SQL Database
- **Frontend:** Bootstrap 5, Bootstrap Icons, Chart.js, custom CSS
- **Cloud:** Azure VM, Azure SQL, Azure Backup

---

## UI Theme

Professional healthcare design with:

- Medical Blue primary (`#1a5f9e`)
- Healthcare Green accent (`#28a745`)
- Fixed sidebar navigation
- Dashboard stat cards and Chart.js analytics
- Responsive tables with search, pagination, and sorting
- Floating-label forms with validation

---

## License

This project is provided for educational and demonstration purposes.
