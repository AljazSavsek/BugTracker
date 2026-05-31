# 🐛 BugTracker

A desktop bug tracking application built with **C# / WPF (.NET 8)** and **MySQL**, developed as a school project for the Programming Applications course.

---

## 📋 Table of Contents

- [About](#about)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Screenshots](#screenshots)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Database Setup (MAMP)](#database-setup-mamp)
- [Configuration](#configuration)
- [Default Login](#default-login)
- [Project Structure](#project-structure)
- [Import / Export](#import--export)
- [Security](#security)
- [License](#license)

---

## About

BugTracker is a Windows desktop application for tracking software bugs and issues within a development team. Users can create, assign, prioritise and resolve bugs, while administrators manage users, categories and view system statistics. All data is stored in a MySQL database.

---

## Features

### 🔐 Authentication
- User login with BCrypt password hashing
- New user registration with field validation
- Role-based access control (Admin / Developer / Tester)
- Session management via `SessionManager`

### 📋 Dashboard
- Live filterable bug list (search by title, filter by status & priority)
- Stat cards showing open / in-progress / resolved counts
- Double-click a row to edit a bug
- Sidebar navigation (Admin menu visible only to Admins)

### ➕ Bug Management
- Add new bugs with title, description, priority, category, assignee and status
- Edit all fields, change status, reassign
- Delete bugs (cascades to comments and history)
- Full change history automatically logged on every save

### 👥 Admin Panel (Admin role only)
| Tab | What you can do |
|-----|-----------------|
| Users | Add, edit, reset password, activate / deactivate |
| Categories | Add, edit, delete (blocked if bugs exist) |
| Statistics | Cards showing totals by status and user counts |
| Settings | Change your own password |

### 📤 Export & Import
| Button | Description |
|--------|-------------|
| 📄 Izvozi TXT | Exports visible bugs as a human-readable report (SaveFileDialog) |
| 📥 Uvozi TXT | Imports bugs from an exported TXT file back into the database |
| 📊 Izvozi Excel | Exports visible bugs to a colour-formatted `.xlsx` file |

> The TXT export and import use **the same format**, so you can export, edit the file, and re-import.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Language | C# 12 |
| Framework | .NET 8 (net8.0-windows) |
| UI | WPF (Windows Presentation Foundation) |
| Database | MySQL 8.x |
| Local DB server | MAMP |
| IDE | Visual Studio 2022 |
| Password hashing | BCrypt.Net-Next 4.0.3 |
| MySQL connector | MySql.Data 9.1.0 |
| Excel export | ClosedXML 0.104.2 |
| Version control | Git / GitHub |

---

## Screenshots

> *Screenshots of all windows are included in the project report (`Bubtracker-tehnicno-porocilo.docx`).*

| Window | Description |
|--------|-------------|
| Login | Username + password login |
| Register | New account creation |
| Dashboard | Bug list with filters and stat cards |
| Add Bug | Form to create a new bug |
| Edit Bug | Edit fields + change history |
| Admin Panel | User / category / stats management |

---

## Prerequisites

Before running the project, make sure you have:

- [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community or higher)  
  → with the **.NET Desktop Development** workload installed
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MAMP](https://www.mamp.info/) with MySQL running on port **3306**
- Internet connection (for NuGet package restore on first build)

---

## Installation

### 1. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/BugTracker.git
cd BugTracker
```

### 2. Open in Visual Studio

Double-click `BugTracker.csproj` or open it via **File → Open → Project/Solution**.

Visual Studio will automatically restore the three NuGet packages on first build.

### 3. Set up the database → [see below](#database-setup-mamp)

### 4. Configure the connection string → [see below](#configuration)

### 5. Run

Press `Ctrl+F5` (Run without debugging). The login window will appear.

---

## Database Setup (MAMP)

### Start MAMP

1. Open **MAMP** and click **Start Servers**.
2. Make sure both **Apache** and **MySQL** are running (green lights).
3. Click **Open WebStart page** → **phpMyAdmin**, or go to:
   ```
   http://localhost:8888/phpMyAdmin
   ```
   > **Note:** MAMP uses port **8888** by default for the web interface,  
   > but MySQL itself runs on port **3306** (or **8889** on older MAMP versions — check MAMP → Preferences → Ports).

### Import the SQL schema

1. In phpMyAdmin click **Import** (top menu).
2. Click **Browse** and select `bugtracker.sql` from the project root.
3. Click **Go**.

The script will:
- Create the `bugtracker` database
- Create all five tables (`UPORABNIKI`, `KATEGORIJE`, `NAPAKE`, `KOMENTARJI`, `ZGODOVINA`)
- Insert sample users, categories and bugs

### MAMP MySQL port

| MAMP version | Default MySQL port |
|--------------|--------------------|
| MAMP 5+ | 3306 |
| MAMP (older) | 8889 |

Check your port under **MAMP → Preferences → Ports → MySQL Port**.

---

## Configuration

Open `Helpers/DatabaseHelper.cs` and update the connection string constant at the top of the class:

```csharp
private const string CS =
    "Server=localhost;Port=3306;Database=bugtracker;Uid=root;Pwd=root;";
```

| Parameter | MAMP default | Change if… |
|-----------|-------------|------------|
| `Server`  | `localhost` | Remote DB server |
| `Port`    | `3306`      | Your MAMP MySQL port is different (e.g. `8889`) |
| `Database`| `bugtracker`| You renamed the database |
| `Uid`     | `root`      | Different MySQL user |
| `Pwd`     | `root`      | MAMP default root password is **root** |

> ⚠️ **MAMP default credentials are `root` / `root`.**  
> This is different from XAMPP where the password is empty.

---

## Default Login

After importing the SQL file, the following accounts are ready to use:

| Username | Password | Role |
|----------|----------|------|
| `admin` | `Admin123!` | Admin |
| `jnovak` | `Admin123!` | Developer |
| `mkovac` | `Admin123!` | Tester |
| `rsitar` | `Admin123!` | Tester |

> Passwords are stored as BCrypt hashes in the database. The plaintext `Admin123!` is never stored.

---

## Project Structure

```
BugTracker/
│
├── BugTracker.csproj          # Project file with NuGet references & DependentUpon links
├── App.xaml                   # Global styles (colours, buttons, inputs)
├── App.xaml.cs
├── bugtracker.sql             # Database schema + sample data
│
├── Models/
│   └── Models.cs              # BugItem, UserItem, CategoryItem, HistoryItem, StatsModel
│
├── Helpers/
│   ├── SessionManager.cs      # Static session holder (UserId, Username, Vloga)
│   ├── DatabaseHelper.cs      # All SQL – login, register, full CRUD
│   ├── ExcelHelper.cs         # .xlsx export via ClosedXML
│   ├── TxtHelper.cs           # TXT export/import (same format)
│   └── InputDialog.cs         # Custom WPF input dialog (replaces VB.InputBox)
│
└── Views/
    ├── LoginWindow.xaml / .cs
    ├── RegisterWindow.xaml / .cs
    ├── Dashboard.xaml / .cs
    ├── DodajBugWindow.xaml / .cs
    ├── UrediiBugWindow.xaml / .cs
    ├── AdminPanelWindow.xaml / .cs
    └── UserDialogs.cs          # DodajUserDialog, UrediUserDialog (code-only WPF)
```

---

## Import / Export

### TXT Export (📄 Izvozi TXT)

Exports currently visible bugs (respecting active filters) as a readable report:

```
================================================================================
  BUGTRACKER – POROČILO O NAPAKAH
  Izvoženo:     29.05.2026  14:30:00
  Skupaj napak: 8
================================================================================

  POVZETEK:
  Odprtih          3
  V delu           2
  ...

── NAPAKA #1 ────────────────────────────────────────────────────────────────────
  Naslov:      Napaka pri prijavi z Google računom
  Status:      Odprt
  Prioriteta:  Visoka
  Kategorija:  Varnost
  Dodeljen:    Jana Novak (jnovak)
  Ustvaril:    Admin Sistemski
  Ustvarjeno:  01.05.2026

  Opis:
    OAuth2 vrne 401.

  [ Opombe za poročilo: ]
  ________________________________________________________________________
```

### TXT Import (📥 Uvozi TXT)

Reads the **exact same format** as the export. The parser:
- Detects `── NAPAKA #N` as a new bug block
- Reads `Naslov:`, `Status:`, `Prioriteta:`, `Kategorija:`, `Dodeljen:`, `Opis:` fields
- Extracts the username from `Dodeljen: Ime Priimek (username)`
- Skips decorative lines (`====`, `────`, `[ Opombe ]`, `____`)

Click **DA** when prompted to generate a sample file to use as a template.

### Excel Export (📊 Izvozi Excel)

Saves an `.xlsx` file to the Desktop with:
- Colour-coded Status column (🔴 open, 🟡 in progress, 🟢 resolved)
- Alternating row background
- Frozen header row
- Auto-fitted column widths

---

## Security

| Mechanism | Implementation |
|-----------|---------------|
| Password hashing | BCrypt with auto-generated salt (work factor 11) |
| SQL injection prevention | All queries use `MySqlCommand.Parameters` |
| Session management | `SessionManager` static class, cleared on logout |
| Role-based access | Admin UI hidden for non-admin roles at runtime |

---

## License

This project was created for educational purposes as part of the **Programming Applications** school course.  
Feel free to use or modify the code for learning purposes.

---

*Built with ❤️ using C# / WPF / MySQL*
