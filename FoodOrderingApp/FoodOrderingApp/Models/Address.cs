using System;
using SQLite;

namespace FoodOrderingApp.Models;

[Table("Addresses")]
public class Address
{
    [PrimaryKey, AutoIncrement]
    public int AddressId { get; set; }

    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    [NotNull, Indexed]
    public int UserId { get; set; }

    [NotNull]
    public string Street { get; set; } = string.Empty;

    [NotNull]
    public string City { get; set; } = string.Empty;

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public string Label { get; set; } = "Home"; // Home, Work, Other

    public bool IsDefault { get; set; } = false;


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

}
