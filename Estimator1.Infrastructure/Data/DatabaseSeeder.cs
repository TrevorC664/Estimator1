using System;
using System.Linq;
using Estimator1.Core.Models;
using Estimator1.Core.Enums;
using Estimator1.Core.Interfaces;
using Estimator1.Infrastructure.Services;
using BC = BCrypt.Net.BCrypt;

namespace Estimator1.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public DatabaseSeeder(ApplicationDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public void SeedData()
        {
            if (!_context.Users.Any())
            {
                _loggingService.Info("No users found in database. Creating seed data...");
                // Add admin user
                _context.Users.Add(new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                    AccessLevel = AccessLevel.Administrator,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                // Add supervisor user
                _context.Users.Add(new User
                {
                    Username = "supervisor",
                    Email = "supervisor@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                    AccessLevel = AccessLevel.Supervisor,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                // Add basic user
                _context.Users.Add(new User
                {
                    Username = "basic",
                    Email = "basic@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                    AccessLevel = AccessLevel.Basic,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                _context.SaveChanges();
                _loggingService.Info("Database seeded successfully with test users.");
            }
        }
    }
}
