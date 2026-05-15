using System;
using SQLite;

namespace FoodOrderingApp.Models;

[Table("Cart")]
public class CartItem
{
    [PrimaryKey, AutoIncrement]
    public int CartItemId { get; set; }

    [NotNull, Indexed]
    public int UserId { get; set; }

    [NotNull, Indexed]
    public int ItemId { get; set; }

    [NotNull]
    public int Quantity { get; set; } = 1;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property (not stored in DB)
    [Ignore]
    public Item? Item { get; set; }
}
