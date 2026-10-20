# FUNewsManagement — Assignment 02 (Razor Pages + SignalR)

A university news management website built with **ASP.NET Core 8 Razor Pages**, **SignalR**, **Entity Framework Core**, and a **3-layer architecture** using the **Repository** and **Singleton** patterns.

> PRN222 — Assignment 02 · ASP.NET Core Razor Pages with SignalR

## Overview

FUNewsManagementSystem lets an institution publish news to the public while giving staff and admins the tools to manage content, categories, accounts and reports.

- Visitors and lecturers can browse **active** news without signing in.
- Staff manage categories, news articles (with tags), their own profile and the news they created.
- Admin manages accounts and produces statistics for a date range.
- News changes are broadcast in **real time** so every open page refreshes without a manual reload.

## Tech stack

| Concern | Choice |
|---|---|
| Framework | ASP.NET Core 8 Razor Pages (`net8.0`) |
| Real-time | SignalR (`NewsHub` at `/newsHub`) |
| ORM | Entity Framework Core 8 (SQL Server provider) |
| Database | Microsoft SQL Server 2012+ |
| Authentication | Cookie authentication + role-based authorization |
| UI | Razor Pages, Bootstrap 5, custom `site.css` |

## Solution structure

```
PhungDangTruong_A02/
├─ BusinessObjects/               Entity classes + EF Core DbContext
├─ DataAccessObjects/             DAOs (Singleton) that query the DbContext
├─ Repositories/                  Repository interfaces + implementations
├─ Services/                      Business logic + service interfaces
├─ PhungDangTruongRazorPages/     ASP.NET Core Razor Pages web app
│  ├─ Pages/                      News, Manage, Categories, Accounts, Reports, Account
│  ├─ Hubs/NewsHub.cs             SignalR hub
│  └─ wwwroot/                    CSS, JS and client libraries
├─ FUNewsManagement.sql           Database creation script
└─ PhungDangTruong_ClassCode_A02.sln
```

Dependencies flow one way:

```
PageModel → Service → Repository → DAO → DbContext
```

Razor pages never talk to EF Core directly; every database operation goes through the service/repository/DAO layers.

## Features

### Public / Lecturer
- Browse active news, filter by category and search by keyword or tag.
- Read the full article on its details page.

### Staff — `AccountRole = 1`
- **Categories:** list, search, create, edit, delete. A category can only be deleted when no news article references it.
- **News articles:** list, search, create, edit, delete, including tag assignment — with real-time updates.
- **History:** view the news articles created by the signed-in staff member.
- **Profile:** view and update their own account information.

### Admin — configured in `appsettings.json`
- **Accounts:** list, search, create, edit, delete system accounts.
- **Reports:** statistics for the period from `StartDate` to `EndDate` (based on the article's created date), sorted in descending order.

Create and update actions are performed in **popup dialogs**, and every delete action asks for **confirmation**.

## Real-time news (SignalR)

- `Hubs/NewsHub.cs` exposes the hub at `/newsHub`.
- When a staff member creates, updates or deletes an article, the page broadcasts:

  ```csharp
  _hub.Clients.All.SendAsync(NewsHub.NewsChanged, action, articleId, title, userName);
  ```

- The client (`wwwroot/js/site.js`) connects with `withAutomaticReconnect()`, listens for `NewsChanged`, shows a toast and re-fetches the current page to swap only the `#newsList` fragment.
- After a reconnect, the list is refreshed again so no updates are missed.

## Getting started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server 2012 or later (Express / Developer / LocalDB)

### 1. Create the database
Run `FUNewsManagement.sql` in SQL Server Management Studio (or with `sqlcmd`). The script creates the `FUNewsManagement` database and the `Category`, `NewsArticle`, `NewsTag`, `SystemAccount` and `Tag` tables.

### 2. Configure the connection string
Edit `PhungDangTruongRazorPages/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\TRUONG;Database=FUNewsManagement;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Change `Server=.\\TRUONG` to match your SQL Server instance.

### 3. Run

```bash
dotnet restore
dotnet run --project PhungDangTruongRazorPages
```

Or open `PhungDangTruong_ClassCode_A02.sln` in Visual Studio and press **F5**.

The **Login page is the default route**. Default URLs: `http://localhost:5066` (HTTP) and `https://localhost:7191` (HTTPS).

## Default admin account

| Email | Password |
|---|---|
| `admin@FUNewsManagementSystem.org` | `@@abc123@@` |

These values live in the `AdminAccount` section of `appsettings.json` and are not stored in the database.

## Roles

| Role | Value | Source |
|---|---|---|
| Admin | `0` | `appsettings.json` (`AdminAccount`) |
| Staff | `1` | `SystemAccount.AccountRole` |
| Lecturer | `2` | `SystemAccount.AccountRole` |

## Notes

- Only news with `NewsStatus = 1` (active) is visible to the public.
- `Category` and `NewsArticle` both use an active/inactive status flag.
- Shared popup/table partials live in `Pages/Manage/` and `Pages/Shared/`; the news sidebar is `Pages/News/_Sidebar.cshtml`.
