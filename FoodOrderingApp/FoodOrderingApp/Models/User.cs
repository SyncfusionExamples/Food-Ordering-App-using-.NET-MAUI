using System;
using SQLite;

namespace FoodOrderingApp.Models;

[Table("Users")]
public class User
{
    [PrimaryKey, AutoIncrement]
    public int UserId { get; set; }

    [Unique, NotNull]
    public string Email { get; set; } = string.Empty;

    [NotNull]
    public string FullName { get; set; } = string.Empty;

    [NotNull]
    public string PasswordHash { get; set; } = string.Empty;

    public string? DOB { get; set; }

    public DateTime JoinDate { get; set; } = DateTime.UtcNow;

    public int RewardsPoints { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
