# Document Management System - .NET 8 API and OpenFGA Integration
This project is a simple document management system built with .NET 8 API, integrated with OpenFGA for managing access permissions for documents. OpenFGA helps manage user permissions on documents, enabling fine-grained access control.

# Project Structure
- API: A RESTful API developed with .NET 8.
- Access Control: OpenFGA is used for fine-grained access control over documents.
- Database: Stores basic information about users and documents.

# Getting Started
The project requires .NET 8 SDK and an OpenFGA access control service.

# Prerequisites
- NET 8 SDK
- OpenFGA
- PostgreSQL (or any other database)
- Docker (for running OpenFGA locally)
- Set Up the Project
- Clone the repository:

```sh
git clone https://github.com/gurkangur/DocumentManagementSystem-OpenFGA.git
```

Restore NuGet packages:
```sh
dotnet restore
```
Set up the database (PostgreSQL):
```sh
dotnet ef database update
```

Run OpenFGA Service:
```sh
docker-compose up
```


# Configuration
Update the OpenFGA settings in the appsettings.json file:

```json
{
  "OpenFga": {
    "StoreId": "your-store-id",
    "ModelId": "your-model-id",
    "ApiUrl": ""http://localhost:8080"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=docs;Username=your_user;Password=your_password"
  }
}
```

# Running the Application
To run the application, execute the following command:

```sh
dotnet run
```
The API will be available at http://localhost:5000 by default.
