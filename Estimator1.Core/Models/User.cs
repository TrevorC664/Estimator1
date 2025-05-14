using System;
using Estimator1.Core.Enums;

namespace Estimator1.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public AccessLevel AccessLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Authentication security properties
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string? LockoutReason { get; set; }
        public string? SecurityStamp { get; set; } // For session/token validation
    }
}
