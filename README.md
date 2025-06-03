# Collection Manager API

A .NET 9 Web API for managing customizable collections with user authentication, authorization, and sharing capabilities.

## Features
- Manage multiple collections, each with custom fields and field types
- User authentication and authorization
- Share collections between users
- OpenAPI (Swagger) documentation

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Running the API

1. Clone the repository
2. Navigate to the project directory
3. Run the API:
   ```sh
   dotnet run --project CollectionManager.API
   ```
4. Access Swagger UI at `https://localhost:<port>/swagger` (if enabled)

## Project Structure
- `CollectionManager.API/` - Main Web API project

## Roadmap
- [ ] Implement authentication and authorization
- [ ] Add user, collection, and sharing models
- [ ] CRUD endpoints for collections and fields
- [ ] Collection sharing endpoints

## License
Copyright (c) 2025 Serhiy Krasovskyy xhunter74@gmail.com  
