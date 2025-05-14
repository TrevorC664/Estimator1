# Estimator1

A WPF application with robust authentication and user management system.

## Features

### Authentication System
- Secure login with BCrypt password hashing
- Failed attempt tracking with lockout
- Real-time feedback on login attempts
- Account lockout after 5 failed attempts

### User Management
- Role-based access control (Basic, Supervisor, Administrator)
- User management interface for administrators
- Secure password validation
- PostgreSQL database integration

### UI Features
- Clean, professional interface
- Smooth window transitions
- Real-time access level feedback
- Proper error handling and user feedback

## Getting Started

### Prerequisites
- .NET 8.0
- PostgreSQL
- Visual Studio 2022 or later

### Test Credentials
- Administrator: admin/test123
- Supervisor: supervisor/test123
- Basic User: basic/test123

### Database Setup
1. Install PostgreSQL
2. Update connection string in `appsettings.json`
3. Run EF Core migrations:
```bash
dotnet ef database update
```

## Project Structure
- **Estimator1.Core**: Core business logic and interfaces
- **Estimator1.Infrastructure**: Data access and services
- **Estimator1.WPF**: User interface and view models
- **Estimator1.Tests**: Unit tests and test utilities

## Security Features
- BCrypt password hashing
- Account lockout protection
- Role-based access control
- Secure password requirements
- Clean session management
