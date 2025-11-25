
# Clinic Management System

A complete RESTful Web API for managing clinic operations including patients, doctors, appointments, and schedules. Built with ASP.NET Core, Entity Framework, and PostgreSQL.

## 🚀 Features

- **JWT Authentication & Authorization**
- **Role-based Access Control** (Admin, Doctor, Receptionist)
- **Patient Management** (CRUD operations with soft delete)
- **Doctor Management** 
- **Appointment Scheduling** with conflict detection
- **Doctor Schedule Management** (Weekly availability)
- **PostgreSQL Database** with EF Core
- **Swagger API Documentation**
- **Repository Pattern & Unit of Work**

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core 6.0
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT with ASP.NET Identity
- **Documentation**: Swagger/OpenAPI
- **Architecture**: Repository Pattern + Unit of Work

## 📋 Prerequisites

- .NET 6.0 SDK
- PostgreSQL Database
- Visual Studio 2022 or VS Code

## ⚙️ Setup & Installation

### 1. Database Configuration
Update `appsettings.json` with your PostgreSQL connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ClinicDB;Username=postgres;Password=yourpassword"
  }
}
