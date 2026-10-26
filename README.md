# Legacies Screen Printers – Backend API

The robust C# backend Web API service for **Legacy Screen Printers**, built using **ASP.NET Core** and connected to a Microsoft SQL Server database. This service acts as the core backend engine handling client authentication, order placement, production tracking, and database querying for the companion Kotlin Android application.

## 🚀 Key Features

* **RESTful Endpoints:** Provides structured JSON endpoints for user authentication, order submission, and real-time status tracking.
* **Database Integration:** Utilizes Entity Framework Core to manage relational data models for print jobs, client orders, and system users via SQL Server.
* **HTTP Request Testing:** Includes native `.http` test files for quick endpoint debugging and verification during local development.
* **Database Scripting:** Includes dedicated SQL initialization and query scripts (`SQLQuery1.sql`) for manual database setup and data management.

## 🛠️ Technology Stack & Architecture

* **Framework:** C#, ASP.NET Core Web API, .NET 8
* **Database/ORM:** Microsoft SQL Server, Entity Framework Core
* **Tooling:** Visual Studio / VS Code, REST Client (`.http` files), Git version control

## 📂 Project Structure

```
LegaciesPrintersApi/
│
├── Properties/           # Launch settings and web server configurations
├── appsettings.json      # Application configuration and SQL connection strings
├── LegaciesPrintersApi.csproj # .NET project file and NuGet package dependencies
├── LegaciesPrintersApi.http   # HTTP request test collection for endpoint debugging
├── Program.cs            # Application entry point, middleware, and dependency injection
├── SQLQuery1.sql         # Custom database query and setup scripts
└── WeatherForecast.cs    # Template / boilerplate diagnostic controller model
```

## ⚙️ Setup & Local Installation

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/ethan1ol/legacies-screen-printers-api.git
   ```

2. **Open the Project:**
   Open `LegaciesPrintersApi.csproj` in Visual Studio or Visual Studio Code.

3. **Configure Connection Strings:**
   Update your database connection string inside `appsettings.json` to point to your SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LegaciesPrintersDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

4. **Initialize the Database:**
   Execute the provided initialization script (`SQLQuery1.sql`) in SQL Server Management Studio (SSMS) or run EF migrations if configured:
   ```bash
   dotnet ef database update
   ```

5. **Run the API:**
   Start the application locally using Visual Studio (`F5`) or via terminal:
   ```bash
   dotnet run
   ```
   The API will start running on its designated local development port (e.g., `https://localhost:5001` or `http://localhost:5000`).

## 🔗 Connecting with the Mobile App
If you are running the companion Android application locally, ensure your mobile app's `RetrofitClient.kt` points to this API's base URL (or use an `ngrok` tunnel if testing from a physical mobile device).
```
