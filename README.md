# DotNetApiUserManagementWithDapper

A RESTful API for user management built with .NET Core and Dapper. This project demonstrates a clean and efficient way to perform CRUD operations using Dapper as the micro-ORM for data access.

## Features

- User registration and management (Create, Read, Update, Delete)
- Uses Dapper for fast and lightweight database interaction
- Implements repository pattern for clean code organization
- Authentication endpoints (login, token validation, etc.)
- Built with .NET Core Web API
- Connects to a relational database (e.g., SQL Server)

## Tech Stack

- **Backend:** C#, .NET Core Web API  
- **ORM:** Dapper  
- **Database:** SQL Server (or any compatible relational DB)  
- **Tools:** Visual Studio, Postman (for testing APIs)  

## Getting Started

### Prerequisites

- [.NET 6 SDK or later](https://dotnet.microsoft.com/download)
- SQL Server or compatible database  
- [Postman](https://www.postman.com/downloads/) (optional, for API testing)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/KevinRen64/DotNetApiUserManagementWithDapper.git
   cd DotNetApiUserManagementWithDapper
2. **Configure your database connection**
   Update the appsettings.json file with your database connection string:
   ```bash
    "ConnectionStrings": {
      "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
    }
3. **Run database migrations or create the required tables manually**
   ```bash
   CREATE TABLE TutorialAppSchema.Users (
     UserId INT IDENTITY(1,1) PRIMARY KEY,
     FirstName NVARCHAR(50),
     LastName NVARCHAR(50),
     Email NVARCHAR(100),
     Gender NVARCHAR(10),
     Active BIT
   );

   CREATE TABLE TutorialAppSchema.UserSalary (
     UserId INT PRIMARY KEY,
     Salary DECIMAL(18, 2),
     FOREIGN KEY (UserId) REFERENCES TutorialAppSchema.Users(UserId)
   );

   CREATE TABLE TutorialAppSchema.Auth (
     UserId INT PRIMARY KEY,
     Email NVARCHAR(100),
     PasswordHash VARBINARY(MAX),
     PasswordSalt VARBINARY(MAX),
     FOREIGN KEY (UserId) REFERENCES TutorialAppSchema.Users(UserId)
   );
4. **Build and run the project**
   ```bash
   dotnet build
   dotnet run
5. **Test the API**
   Use Swagger UI or Postman to explore and test the API endpoints below:
   
   User Management
   | HTTP Method | Endpoint                      | Description                   |
   | ----------- | ----------------------------- | ----------------------------- |
   | GET         | `/User/TestConnection`        | Tests database connectivity   |
   | GET         | `/User/GetUsers`              | Retrieves all users           |
   | GET         | `/User/GetUsers/{id}`         | Retrieves a user by userId    |
   | POST        | `/User/AddUser`               | Adds a new user               |
   | PUT         | `/User/EditUser`              | Updates an existing user      |
   | DELETE      | `/User/DeleteUser/{id}`       | Deletes a user by userId      |

   Salary Management
   | HTTP Method | Endpoint                      | Description                   |
   | ----------- | ----------------------------- | ----------------------------- |
   | GET         | `/User/GetUsersSalary`        | Retrieves all users’ salaries |
   | GET         | `/User/GetUsersSalary/{id}`   | Retrieves salary by userId    |
   | POST        | `/User/AddUserSalary`         | Adds salary record for a user |
   | PUT         | `/User/EditUserSalary`        | Updates salary for a user     |
   | DELETE      | `/User/RemoveUserSalary/{id}` | Deletes salary by userId      |

   Auth Management
   | HTTP Method | Endpoint                      | Description                           |
   | ----------- | ----------------------------- | -----------------------------         |
   | POST        | `/Auth/Register` | Registers a new user and stores hashed credentials |
   | POST        | `/Auth/Login`    | Verifies credentials and returns success or error  |


## Project Structure
  Controllers/ — API controllers handling HTTP requests
  Data/ # Dapper database context and helper classes
  Dtos/ # Data Transfer Objects (DTOs) for user input/output
  Models/ — Data models representing users
  appsettings.json — Configuration file
  


  
