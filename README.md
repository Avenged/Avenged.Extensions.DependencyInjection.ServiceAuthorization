# ServiceAuthorization

A lightweight .NET library that adds automatic role-based authorization to service classes using Castle DynamicProxy and ASP.NET Core's `IAuthorizationService`.

Easily protect your service methods by decorating them with `[Authorize]` attributes, just like in MVC controllers.

---

## ✨ Features

- Intercepts service method calls and checks authorization before execution.
- Supports `[Authorize]` attributes on class and method level.
- Fully compatible with ASP.NET Core authorization and role-based policies.
- Simple integration with `IServiceCollection`.

---

## 📦 Installation

Install via NuGet:

```
dotnet add package Avenged.Extensions.DependencyInjection.ServiceAuthorization
```

---

## ⚙️ Requirements

- .NET Standard 2.1 or .NET 6.0+
- ASP.NET Core's authorization system must be set up
- Your project must already reference:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authorization" />
```

---

## 🚀 Getting Started

### 1. Register authorization services

In your `Startup.cs` (for .NET Core 3.1/5) or `Program.cs` (for .NET 6/7/8):

```csharp
services.AddAuthorization(); // if not already added
services.AddHttpContextAccessor();

services.AddServiceAuthorization();
```

---

### 2. Register your service with authorization support

```csharp
services.AddAuthorizedService<IProductService, ProductService>();
```

---

### 3. Use `[Authorize]` on your service

```csharp
[Authorize(Roles = "Admin")]
public class ProductService : IProductService
{
    public void DeleteProduct(int id)
    {
        // This method requires an Admin role
    }

    [Authorize(Roles = "Manager")]
    public void UpdateProduct(int id)
    {
        // This method requires a Manager role
    }
}
```

> Authorization will be evaluated automatically before method execution. If unauthorized, an `UnauthorizedAccessException` will be thrown.

---

## 🔐 How It Works

- A dynamic proxy wraps your service implementation.
- The proxy intercepts calls and reads any `[Authorize]` attributes present.
- It checks the current user via `HttpContext` and calls `IAuthorizationService` to enforce role or policy requirements.

---

## 📎 Notes

- Dependencies such as `Castle.Core`, `Microsoft.AspNetCore.Authorization`, and `Microsoft.AspNetCore.Http.Abstractions` are marked as `PrivateAssets="all"` to avoid version conflicts. You must ensure your application references any necessary packages directly.

---

## 🧪 Example Use Case

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddServiceAuthorization();
builder.Services.AddAuthorizedService<IMyService, MyService>();

var app = builder.Build();
```

---

## 📄 License

MIT
