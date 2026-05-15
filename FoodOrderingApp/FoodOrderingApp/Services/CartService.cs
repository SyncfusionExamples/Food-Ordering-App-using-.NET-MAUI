using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FoodOrderingApp.Models;

namespace FoodOrderingApp.Services;

public class CartService : ICartService
{
    private readonly IDatabaseService _databaseService;
    private readonly IAuthService _authService;

    public CartService(IDatabaseService databaseService, IAuthService authService)
    {
        _databaseService = databaseService;
        _authService = authService;
    }

    public async Task<bool> AddToCartAsync(int itemId, int quantity)
    {
        try
        {
            var userId = _authService.GetCurrentUserId();
            if (userId == null)
                return false;

            // Check if item already in cart
            var existingItems = await _databaseService.QueryAsync<CartItem>(
                "SELECT * FROM CartItems WHERE UserId = ? AND ItemId = ?", userId, itemId);

            if (existingItems.Any())
            {
                // Update quantity
                var cartItem = existingItems.First();
                cartItem.Quantity += quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
                await _databaseService.UpdateAsync(cartItem);
            }
            else
            {
                // Insert new cart item
                var cartItem = new CartItem
                {
                    UserId = userId.Value,
                    ItemId = itemId,
                    Quantity = quantity,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _databaseService.InsertAsync(cartItem);
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error adding to cart: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RemoveFromCartAsync(int cartItemId)
    {
        try
        {
            var cartItem = await _databaseService.GetByIdAsync<CartItem>(cartItemId);
            if (cartItem == null)
                return false;

            await _databaseService.DeleteAsync(cartItem);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error removing from cart: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateQuantityAsync(int cartItemId, int quantity)
    {
        try
        {
            if (quantity < 1 || quantity > 99)
                return false;

            var cartItem = await _databaseService.GetByIdAsync<CartItem>(cartItemId);
            if (cartItem == null)
                return false;

            cartItem.Quantity = quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            await _databaseService.UpdateAsync(cartItem);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating quantity: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ClearCartAsync()
    {
        try
        {
            var userId = _authService.GetCurrentUserId();
            if (userId == null)
                return false;

            var cartItems = await _databaseService.QueryAsync<CartItem>(
                "SELECT * FROM CartItems WHERE UserId = ?", userId);

            foreach (var item in cartItems)
            {
                await _databaseService.DeleteAsync(item);
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error clearing cart: {ex.Message}");
            return false;
        }
    }

    public async Task<List<CartItem>> GetCartItemsAsync()
    {
        try
        {
            var userId = _authService.GetCurrentUserId();
            if (userId == null)
                return new List<CartItem>();

            var cartItems = await _databaseService.QueryAsync<CartItem>(
                "SELECT * FROM CartItems WHERE UserId = ? ORDER BY AddedAt DESC", userId);

            return cartItems;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting cart items: {ex.Message}");
            return new List<CartItem>();
        }
    }

    public async Task<decimal> CalculateTotalAsync()
    {
        try
        {
            var userId = _authService.GetCurrentUserId();
            if (userId == null)
                return 0;

            // Get all cart items with their prices from Items table
            var cartItems = await GetCartItemsAsync();
            decimal total = 0;

            foreach (var cartItem in cartItems)
            {
                var item = await _databaseService.GetByIdAsync<Item>(cartItem.ItemId);
                if (item != null)
                {
                    total += item.Price * cartItem.Quantity;
                }
            }

            return total;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error calculating total: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> GetCartCountAsync()
    {
        try
        {
            var cartItems = await GetCartItemsAsync();
            return cartItems.Sum(item => item.Quantity);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting cart count: {ex.Message}");
            return 0;
        }
    }
}
