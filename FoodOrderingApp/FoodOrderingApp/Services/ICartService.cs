using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodOrderingApp.Models;

namespace FoodOrderingApp.Services;

public interface ICartService
{
    Task<bool> AddToCartAsync(int itemId, int quantity);
    Task<bool> RemoveFromCartAsync(int cartItemId);
    Task<bool> UpdateQuantityAsync(int cartItemId, int quantity);
    Task<bool> ClearCartAsync();
    Task<List<Models.CartItem>> GetCartItemsAsync();
    Task<decimal> CalculateTotalAsync();
    Task<int> GetCartCountAsync();
}
