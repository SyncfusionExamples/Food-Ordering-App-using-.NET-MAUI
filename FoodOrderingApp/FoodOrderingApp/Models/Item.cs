using System;
using SQLite;

namespace FoodOrderingApp.Models;

[Table("Items")]
public class Item
{
    [PrimaryKey, AutoIncrement]
    public int ItemId { get; set; }

    [NotNull]
    public string RestaurantName { get; set; } = string.Empty;

    [NotNull]
    public string ItemName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [NotNull]
    public decimal Price { get; set; }

    public string? Image { get; set; }

    public bool IsVeg { get; set; } = false;

    public string Cuisine { get; set; } = string.Empty;

    public double Rating { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
