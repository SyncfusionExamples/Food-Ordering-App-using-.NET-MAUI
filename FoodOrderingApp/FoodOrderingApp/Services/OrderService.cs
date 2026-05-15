using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FoodOrderingApp.Models;

namespace FoodOrderingApp.Services;

public class OrderService : IOrderService
{
    private readonly IDatabaseService _databaseService;
    private readonly IAuthService _authService;
    private readonly ICartService _cartService;
    private const decimal RewardsPercentage = 0.05m;

    public OrderService(IDatabaseService databaseService, IAuthService authService, ICartService cartService)
    {
        _databaseService = databaseService;
        _authService = authService;
        _cartService = cartService;
    }

    public async Task<Order?> CreateOrderAsync(decimal totalAmount, int? addressId = null)
    {
        try
        {
            var userId = _authService.GetCurrentUserId();
            if (userId == null)
                return null;

            // Get cart items before clearing
            var cartItems = await _cartService.GetCartItemsAsync();
            if (!cartItems.Any())
                return null;

            Order? createdOrder = null;

            // Execute transaction
            await _databaseService.ExecuteTransactionAsync(async () =>
            {
                // Create order
                var order = new Order
                {
                    UserId = userId.Value,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Status = "Confirmed",
                    EstimatedDelivery = DateTime.UtcNow.AddMinutes(45),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var orderId = await _databaseService.InsertAsync(order);
                order.OrderId = orderId;
                createdOrder = order;

                // Create order items (snapshot of cart items with current prices)
                var orderItems = new List<OrderItem>();
                foreach (var cartItem in cartItems)
                {
                    var item = await _databaseService.GetByIdAsync<Item>(cartItem.ItemId);
                    if (item != null)
                    {
                        orderItems.Add(new OrderItem
                        {
                            OrderId = orderId,
                            ItemId = cartItem.ItemId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = item.Price,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _databaseService.InsertAllAsync(orderItems);

                // Calculate and update user rewards
                var rewards = await CalculateRewardsAsync(totalAmount);
                var user = await _authService.GetCurrentUserAsync();
                if (user != null)
                {
                    user.RewardsPoints += (int)rewards;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _databaseService.UpdateAsync(user);
                }

                // Clear cart
                await _cartService.ClearCartAsync();
            });

            return createdOrder;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating order: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
    {
        try
        {
            var order = await _databaseService.GetByIdAsync<Order>(orderId);
            if (order == null)
                return false;

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            if (status == "Delivered")
            {
                order.DeliveredAt = DateTime.UtcNow;
            }

            await _databaseService.UpdateAsync(order);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating order status: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Order>> GetUserOrdersAsync()
    {
        try
        {
            var userId = _authService.GetCurrentUserId();
            if (userId == null)
                return new List<Order>();

            var orders = await _databaseService.QueryAsync<Order>(
                "SELECT * FROM Orders WHERE UserId = ? ORDER BY OrderDate DESC", userId);

            return orders;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting user orders: {ex.Message}");
            return new List<Order>();
        }
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        try
        {
            var userId = _authService.GetCurrentUserId();
            if (userId == null)
                return null;

            var orders = await _databaseService.QueryAsync<Order>(
                "SELECT * FROM Orders WHERE OrderId = ? AND UserId = ?", orderId, userId);

            return orders.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting order: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        try
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null || order.Status != "Confirmed")
                return false;

            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;
            await _databaseService.UpdateAsync(order);

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cancelling order: {ex.Message}");
            return false;
        }
    }

    public async Task<decimal> CalculateRewardsAsync(decimal totalAmount)
    {
        // Simulate async operation
        await Task.Delay(50);
        return totalAmount * RewardsPercentage;
    }

    public async Task<string> SimulateStatusUpdateAsync(int orderId)
    {
        try
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null)
                return order?.Status ?? "Unknown";

            // Simulate progression through order stages
            var statusProgression = new[] { "Confirmed", "Preparing", "OutForDelivery", "Delivered" };
            var currentIndex = Array.IndexOf(statusProgression, order.Status);

            // If already delivered, don't update
            if (currentIndex >= statusProgression.Length - 1)
                return order.Status;

            // Advance to next status
            var nextStatus = statusProgression[currentIndex + 1];
            await UpdateOrderStatusAsync(orderId, nextStatus);

            return nextStatus;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error simulating status update: {ex.Message}");
            return "Unknown";
        }
    }
}
