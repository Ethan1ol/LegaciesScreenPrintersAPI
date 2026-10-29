# Legacies Screen Printers API

A RESTful backend service built with ASP.NET Core and Entity Framework Core to support screen-printing orders, customer management, inventory/materials, and live order tracking for the Legacies Screen Printers mobile app.

---

## 📋 Features

- **Authentication & User Management**: User registration and login endpoints with password hashing.
- **Order Processing & Persistence**: End-to-end creation, status management, and querying of customer orders and item breakdowns.
- **Real-Time Tracking**: Fast tracking-number lookup endpoint (`/api/orders/track/{trackingNumber}`) for the mobile client.
- **Materials Inventory**: Querying and updating raw material stock (inks, apparel, vinyl).
- **Interactive Documentation**: Swagger UI enabled for testing all API routes interactively.

---

## 🛠️ Prerequisites

- **.NET SDK**: .NET 9.0 SDK or higher installed
- **Database Engine**: Microsoft SQL Server (LocalDB, SQL Express, or full SQL Server instance)
- **Tools**: SQL Server Management Studio (SSMS) or Azure Data Studio

---

## 🗄️ Database Setup

1. Open your SQL Server client (such as SSMS).
2. Connect to your local SQL Server instance.
3. Open and run the included `SQLQuery1.sql` script to create the database schema and initialize the tables:
   - `users`
   - `materials`
   - `orders`
   - `order_items`
4. Confirm your connection string in `appsettings.json` matches your local SQL Server instance:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=LegaciesScreenPrintersDb;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

---

## 🚀 Running the API

1. Open a terminal in the root directory of this repository:
   ```bash
   dotnet restore
   dotnet build
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. By default, the API will listen on `http://localhost:5000`.
4. Access the Swagger documentation in your browser:
   ```
   http://localhost:5000/swagger
   ```

---

## 📱 Connecting the Android App (`LegaciesScreenPrintersAPK`)

### 1. Android Studio Emulator
When running the Android application inside an **Android Studio Emulator**, the Android OS routes network requests to your computer's `localhost` via the special IP alias `10.0.2.2`. In `RetrofitClient.kt`:
```kotlin
private const val BASE_URL = "http://10.0.2.2:5000/"
```

### 2. Physical Mobile Device (Important)
If you are running the app on a **physical mobile phone** instead of the emulator, `10.0.2.2` will not connect. You must point the app to your development computer's local Wi-Fi/LAN IPv4 address.

In `RetrofitClient.kt`, update the base URL to your PC's local IP address:
```kotlin
private const val BASE_URL = "http://192.168.1.50:5000/"
```

> **Note**:
> - Replace `192.168.1.50` with your PC's actual IPv4 address (run `ipconfig` in Command Prompt / PowerShell to check).
> - Make sure both your computer and your phone are connected to the same Wi-Fi network.
> - Ensure your Windows Firewall allows inbound connections on port `5000`.
