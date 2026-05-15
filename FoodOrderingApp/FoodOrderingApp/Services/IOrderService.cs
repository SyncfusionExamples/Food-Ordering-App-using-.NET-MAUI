using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodOrderingApp.Models;

namespace FoodOrderingApp.Services;

public interface IOrderService
{
    Task<Order?> CreateOrderAsync(decimal totalAmount, int? addressId = null);
    Task<bool> UpdateOrderStatusAsync(int orderId, string status);
    Task<List<Order>> GetUserOrdersAsync();
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<bool> CancelOrderAsync(int orderId);
    Task<decimal> CalculateRewardsAsync(decimal totalAmount);

    /// <summary>
    /// Simulate advancing order status through stages: Confirmed → Preparing → OutForDelivery → Delivered
    /// </summary>
    Task<string> SimulateStatusUpdateAsync(int orderId);
}
