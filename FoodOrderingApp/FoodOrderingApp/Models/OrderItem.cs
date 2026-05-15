using System;
using SQLite;

namespace FoodOrderingApp.Models;

[Table("OrderItems")]
public class OrderItem
{
    [PrimaryKey, AutoIncrement]
    public int OrderItemId { get; set; }

    [NotNull, Indexed]
    public int OrderId { get; set; }

    [NotNull, Indexed]
    public int ItemId { get; set; }

    [NotNull]
    public int Quantity { get; set; }

    [NotNull]
    public decimal UnitPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; }

    // Navigation property (not stored in DB)
    [Ignore]
    public Item? Item { get; set; }
}
