# FileMaster Web API

FileMaster is a modern, high-performance **ASP.NET Core Web API** built with **.NET 10** for secure, user-centric file management. It features automated background processing for image thumbnails, JWT-based security, and isolated user storage, while ensuring high observability and performance.

## Key Professional Features

- **Built on .NET 10:** Leveraging the latest performance enhancements and C# 14 features.
- **JWT Authentication:** Secure user registration and login using ASP.NET Core Identity with 256-bit encryption.
- **Performance Optimization:** Integrated **IMemoryCache** to reduce database load and provide lightning-fast file listing.
- **Observability (Logging):** Structured logging with **Serilog**. All activities and errors are tracked in hem Console hem de rolling File logs for easy debugging.
- **Background Processing:** Asynchronous thumbnail generation via **Hangfire** and **ImageSharp** to keep the API responsive.
- **Quality Assurance:** Robust **Unit Testing** suite with **xUnit** and **Moq**, ensuring business logic reliability for file uploads and security constraints.
- **User Isolation:** Total data privacy; users can only interact with their own files.
- **Security Constraints:** File extension whitelisting (.jpg, .png, .pdf) and a 10MB file size limit.

## Tech Stack

* **Framework:** .NET 10 (ASP.NET Core API)
* **Database & ORM:** SQL Server & Entity Framework Core 10
* **Security:** JWT & ASP.NET Core Identity
* **Monitoring:** Serilog (Structured Logging)
* **Caching:** IMemoryCache
* **Background Jobs:** Hangfire
* **Testing:** xUnit & Moq
* **Image Processing:** SixLabors.ImageSharp

## Testing
The project includes a dedicated test suite to ensure system stability:

- **Controller Tests:** Mocked services using Moq to test API responses.
- **Logic Tests:** Validation of file size limits and allowed extensions.

## Monitoring & Performance
* **Logs:** Located in the `/Logs` directory, categorized by date.
* **Cache Policy:** File lists are cached for 5 minutes, automatically invalidated on new uploads to ensure data consistency.

## Getting Started

### 1. Prerequisites
- .NET 10 SDK
- SQL Server (LocalDB or Express)

### 2. Installation
1. Update `appsettings.json` with your ConnectionString.
2. Run migrations: `dotnet ef database update`
3. Start the app: `dotnet run`
