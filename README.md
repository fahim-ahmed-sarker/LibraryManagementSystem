# Library Management System

এটি ASP.NET Core MVC, Entity Framework Core এবং ASP.NET Core Identity দিয়ে তৈরি একটি Library Management System। এতে librarian এবং member-এর জন্য বই, borrowing, reservation, fine, feedback, profile এবং report management-এর সুবিধা আছে।

**GitHub Repository:** [fahim-ahmed-sarker/LibraryManagementSystem](https://github.com/fahim-ahmed-sarker/LibraryManagementSystem)

## Website Preview

Project-এর visual preview হিসেবে repository-তে থাকা logo এবং book cover images নিচে দেওয়া হলো:

![City Knowledge Library logo](LibraryManagementSystem/wwwroot/images/logo/library-logo.png)

| Book catalogue preview | Book catalogue preview | Book catalogue preview | Book catalogue preview |
|---|---|---|---|
| ![1984](LibraryManagementSystem/wwwroot/images/covers/1984.png) | ![Atomic Habits](LibraryManagementSystem/wwwroot/images/covers/atomic-habits.png) | ![Dune](LibraryManagementSystem/wwwroot/images/covers/dune.png) | ![Clean Code](LibraryManagementSystem/wwwroot/images/covers/clean-code.png) |

Application-এ ব্যবহৃত প্রধান screen এবং workflow:

- Home page: library introduction, featured books এবং new arrivals
- Book Catalogue: search, genre filter, availability status, details এবং edit
- Book Details: cover image, author, ISBN, summary, ratings এবং reviews
- Reports Dashboard: total books, members, borrowings, overdue books এবং fines
- Account: member/librarian registration, login, profile settings এবং password change

### Home Page

![Library Management System home page](LibraryManagementSystem/wwwroot/images/screenshots/home-page.png)

### Book Catalogue

![Book catalogue with search and genre filter](LibraryManagementSystem/wwwroot/images/screenshots/book-catalogue.png)

### Book Details

![Book details, availability, ratings and reviews](LibraryManagementSystem/wwwroot/images/screenshots/book-details.png)

### Reports Dashboard

![Reports dashboard with library statistics](LibraryManagementSystem/wwwroot/images/screenshots/reports-dashboard.png)

> GitHub-এর image preview দেখতে উপরের repository link খুলুন। Local application চালানোর পর browser-এ `https://localhost:7070` খুললে সম্পূর্ণ interactive website দেখা যাবে।

## Technology Stack

- .NET 8 / ASP.NET Core MVC
- Entity Framework Core 8
- Microsoft SQL Server / LocalDB
- ASP.NET Core Identity
- xUnit এবং EF Core InMemory দিয়ে unit test

## প্রয়োজনীয় Tools

### সবার জন্য

1. **Git**: GitHub থেকে repository clone করার জন্য
2. **.NET 8 SDK**: project build এবং run করার জন্য
3. **SQL Server LocalDB** অথবা **SQL Server Express/Developer**: application database-এর জন্য
4. **একটি browser**: Chrome, Edge অথবা Firefox

### Visual Studio Code-এর জন্য

- Visual Studio Code
- **C# Dev Kit** extension
- **C#** extension, যদি C# Dev Kit ইনস্টল করার সময় এটি স্বয়ংক্রিয়ভাবে না আসে
- SQL database দেখতে চাইলে **SQL Server (mssql)** extension

### Visual Studio-এর জন্য

- Visual Studio 2022 (17.8 বা পরবর্তী version recommended)
- Visual Studio Installer থেকে **ASP.NET and web development** workload
- SQL Server LocalDB সাধারণত Visual Studio-এর সঙ্গে ইনস্টল হয়। না থাকলে SQL Server Express বা Developer edition ইনস্টল করুন।

> Node.js, npm বা আলাদা frontend build tool প্রয়োজন নেই। এই project-এর frontend ASP.NET Core Razor Views এবং static files দিয়ে তৈরি।

## GitHub থেকে Project Clone

PowerShell বা Command Prompt খুলে চালান:

```powershell
git clone https://github.com/fahim-ahmed-sarker/LibraryManagementSystem.git
cd LibraryManagementSystem
```

Repository clone করার পর solution structure সাধারণত এমন হবে:

```text
LibraryManagementSystem/
  LibraryManagementSystem.slnx
  LibraryManagementSystem/
  LibraryManagementSystem.Tests/
```

## Dependencies Restore এবং Build

Solution folder-এ থেকে চালান:

```powershell
dotnet restore
dotnet build
```

Project-এর প্রধান NuGet dependencies:

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 8.0.0
- `Microsoft.AspNetCore.Identity.UI` 8.0.0
- `Microsoft.EntityFrameworkCore.Design` 8.0.0
- `Microsoft.EntityFrameworkCore.SqlServer` 8.0.0
- `Microsoft.EntityFrameworkCore.Tools` 8.0.0

Test project-এর dependencies:

- `Microsoft.NET.Test.Sdk`
- `xunit`
- `xunit.runner.visualstudio`
- `coverlet.collector`
- `Microsoft.EntityFrameworkCore.InMemory`

## Database Configuration

### Option 1: SQL Server LocalDB (Default)

Application-এর default connection string [appsettings.json](LibraryManagementSystem/appsettings.json)-এ আছে:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LibraryManagementSystemDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

LocalDB ইনস্টল আছে কি না PowerShell-এ যাচাই করুন:

```powershell
sqllocaldb info
```

`MSSQLLocalDB` না চললে চালু করুন:

```powershell
sqllocaldb start MSSQLLocalDB
```

তারপর application run করলে database `LibraryManagementSystemDb` তৈরি হবে এবং existing EF Core migrations automatically apply হবে।

### Option 2: SQL Server Express বা Developer

SQL Server ব্যবহার করলে [appsettings.Development.json](LibraryManagementSystem/appsettings.Development.json)-এ সরাসরি password বা production secret commit না করে User Secrets ব্যবহার করা ভালো। Development environment-এ connection string set করার command:

```powershell
dotnet user-secrets init --project .\LibraryManagementSystem\LibraryManagementSystem.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\\SQLEXPRESS;Database=LibraryManagementSystemDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" --project .\LibraryManagementSystem\LibraryManagementSystem.csproj
```

আপনার SQL Server instance ভিন্ন হলে `Server=`-এর value পরিবর্তন করুন। উদাহরণ:

```text
Server=localhost;Database=LibraryManagementSystemDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

SQL username/password ব্যবহার করলে connection string-এ `User Id` এবং `Password` দিন, তবে তা source control-এ commit করবেন না।

### Migration সম্পর্কিত তথ্য

Application startup-এর সময় `Database.MigrateAsync()` চলে। তাই সাধারণভাবে আলাদা migration command লাগবে না। প্রয়োজন হলে solution folder থেকে manually চালানো যাবে:

```powershell
dotnet ef database update --project .\LibraryManagementSystem\LibraryManagementSystem.csproj
```

`dotnet ef` command না পাওয়া গেলে:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

## Application Run: Visual Studio Code

1. VS Code-এ cloned root folder `LibraryManagementSystem` খুলুন।
2. C# Dev Kit extension ইনস্টল করুন।
3. LocalDB অথবা SQL Server চালু আছে নিশ্চিত করুন।
4. VS Code-এর integrated terminal খুলে চালান:

```powershell
dotnet restore
dotnet build
dotnet run --project .\LibraryManagementSystem\LibraryManagementSystem.csproj
```

5. Browser-এ এই URL খুলুন:

```text
https://localhost:7070
```

Certificate warning এলে development certificate trust করুন:

```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

HTTPS ব্যবহার না করতে চাইলে:

```powershell
dotnet run --project .\LibraryManagementSystem\LibraryManagementSystem.csproj --launch-profile http
```

তারপর `http://localhost:5289` খুলুন। VS Code-এ `Run and Debug` ব্যবহার করতে চাইলে C# Dev Kit-এর project launch profile থেকে `https` বা `http` profile নির্বাচন করুন।

## Application Run: Visual Studio

1. Visual Studio 2022 খুলুন।
2. `File > Open > Project/Solution` নির্বাচন করুন।
3. `LibraryManagementSystem.slnx` খুলুন।
4. Solution-এ web project `LibraryManagementSystem`-কে Startup Project করুন।
5. LocalDB অথবা SQL Server চালু আছে নিশ্চিত করুন।
6. `Build > Restore NuGet Packages` এবং তারপর `Build > Build Solution` নির্বাচন করুন।
7. toolbar-এর profile থেকে `https` নির্বাচন করে `F5` চাপুন।

Visual Studio সাধারণত browser-এ `https://localhost:7070` খুলবে। HTTP profile চালাতে চাইলে `http` নির্বাচন করে run করুন; তখন URL হবে `http://localhost:5289`।

## Test Run

সব test চালাতে:

```powershell
dotnet test
```

Test project-এর জন্য real SQL Server লাগে না; borrowing service tests EF Core InMemory database ব্যবহার করে।

## Initial Usage

- Member account তৈরি করতে `/Account/RegisterMember` খুলুন।
- Librarian account তৈরি করতে `/Account/RegisterLibrarian` খুলুন।
- প্রথম librarian registration-এর জন্য configuration-এ থাকা code ব্যবহার করুন: `LIBRARY2026`।
- Librarian registration code production environment-এ পরিবর্তন করে User Secrets বা environment variable ব্যবহার করা উচিত।

## Useful URLs

| কাজ | URL |
|---|---|
| Home | `/` |
| Login | `/Account/Login` |
| Member registration | `/Account/RegisterMember` |
| Librarian registration | `/Account/RegisterLibrarian` |
| Profile settings | `/Account/ProfileSettings` |
| Change password | `/Account/ChangePassword` |
| Librarian reports | `/Reports` |

## Troubleshooting

### `Cannot connect to server` বা database error

- LocalDB চালু আছে কি না দেখুন: `sqllocaldb info MSSQLLocalDB`
- SQL Server service চালু আছে কি না দেখুন।
- connection string-এর `Server=` value আপনার instance অনুযায়ী পরিবর্তন করুন।
- পুরনো broken database হলে LocalDB-তে database drop করে আবার application চালানো যায়; এতে local data মুছে যাবে।

### `dotnet` command পাওয়া যাচ্ছে না

.NET 8 SDK ইনস্টল করে নতুন terminal খুলুন:

```powershell
dotnet --version
```

Version `8.x.x` দেখা উচিত। শুধু .NET Runtime ইনস্টল থাকলে build করা যাবে না; SDK প্রয়োজন।

### HTTPS certificate error

```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

তারপর application পুনরায় run করুন।

### Port already in use

অন্য application port ব্যবহার করলে `Properties/launchSettings.json`-এ `5289` এবং `7070` পরিবর্তন করুন, অথবা নির্দিষ্ট port দিয়ে run করুন:

```powershell
dotnet run --project .\LibraryManagementSystem\LibraryManagementSystem.csproj --urls "http://localhost:5099"
```

## Important Security Notes

- `appsettings.json`-এর default librarian code এবং development connection string production-এ ব্যবহার করবেন না।
- Production secret, SQL password বা API key GitHub-এ commit করবেন না।
- Production database-এর জন্য environment variables, User Secrets বা secure secret manager ব্যবহার করুন।
- Database backup রাখুন, বিশেষ করে migration বা schema পরিবর্তনের আগে।