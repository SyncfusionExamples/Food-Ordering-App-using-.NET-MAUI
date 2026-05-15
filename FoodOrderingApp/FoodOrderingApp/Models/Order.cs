using System;
using System.Collections.Generic;
using SQLite;

namespace FoodOrderingApp.Models;

[Table("Orders")]
public class Order
{
    [PrimaryKey, AutoIncrement]
    public int OrderId { get; set; }

    [NotNull, Indexed]
    public int UserId { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [NotNull]
    public decimal TotalAmount { get; set; }

    [NotNull]
    public string Status { get; set; } = "Confirmed"; // Confirmed, Preparing, OutForDelivery, Delivered

    public DateTime? EstimatedDelivery { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public int? DeliveryPartnerId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property (not stored in DB)
    [Ignore]
    public List<OrderItem>? Items { get; set; }
}
