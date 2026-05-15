using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FoodOrderingApp.Models;
using FoodOrderingApp.Services;
using Microsoft.Maui.Storage;
using SQLite;

namespace FoodOrderingApp.Database;

public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _database;
    private const string DatabaseFileName = "foodordering.db3";
    private string _databasePath = string.Empty;

    public DatabaseService()
    {
        _databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);
    }

    public async Task InitializeAsync()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(_databasePath);

        // Enable foreign keys
        await _database.ExecuteAsync("PRAGMA foreign_keys = ON");

        // Create tables
        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<Item>();
        await _database.CreateTableAsync<CartItem>();
        await _database.CreateTableAsync<Order>();
        await _database.CreateTableAsync<OrderItem>();
        await _database.CreateTableAsync<Address>();

        // Create indexes
        await CreateIndexesAsync();

        // Seed initial data if empty
        var itemCount = await _database.Table<Item>().CountAsync();
        if (itemCount == 0)
        {
            await SeedDataAsync();
        }
    }

    private async Task CreateIndexesAsync()
    {
        if (_database == null) return;

        // Create indexes for frequently queried columns
        await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email)");
        await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_items_veg ON Items(IsVeg)");
        await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_items_cuisine ON Items(Cuisine)");
        await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_cart_userid ON Cart(UserId)");
        await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_cart_itemid ON Cart(ItemId)");
        await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_orders_userid ON Orders(UserId)");
        await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_orders_status ON Orders(Status)");
        await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_orderitems_orderid ON OrderItems(OrderId)");
        await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_addresses_userid ON Addresses(UserId)");
    }

    private async Task SeedDataAsync()
    {
        if (_database == null) return;

        var items = new List<Item>
        {
            new() { RestaurantName = "The Burger Loft", ItemName = "Classic Burger", Description = "Juicy beef patty with fresh veggies", Price = 6.50m, IsVeg = false, Cuisine = "American", Rating = 4.8, Image = "burger.jpg" },
            new() { RestaurantName = "Pizzeria Artisan", ItemName = "Margherita Pizza", Description = "Fresh mozzarella, basil, tomato", Price = 12.99m, IsVeg = true, Cuisine = "Italian", Rating = 4.6, Image = "pizza.jpg" },
            new() { RestaurantName = "Sakura Sushi Bar", ItemName = "Salmon Poke Bowl", Description = "Brown rice, sesame dressing, avocado", Price = 18.25m, IsVeg = false, Cuisine = "Japanese", Rating = 4.9, Image = "sushi.jpg" },
            new() { RestaurantName = "Noodle Theory", ItemName = "Pad Thai", Description = "Rice noodles with peanut sauce", Price = 14.50m, IsVeg = true, Cuisine = "Thai", Rating = 4.5, Image = "noodles.jpg" },
            new() { RestaurantName = "Green Garden", ItemName = "Buddha Bowl", Description = "Mixed veggies, quinoa, tahini dressing", Price = 11.99m, IsVeg = true, Cuisine = "Healthy", Rating = 4.7, Image = "bowl.jpg" },
            new() { RestaurantName = "Smoke & Fire BBQ", ItemName = "Pulled Pork Sandwich", Description = "Slow-smoked pork with coleslaw", Price = 13.50m, IsVeg = false, Cuisine = "American", Rating = 4.4, Image = "bbq.jpg" },
            new() { RestaurantName = "Indus Spice", ItemName = "Butter Chicken", Description = "Tender chicken in creamy tomato sauce", Price = 15.99m, IsVeg = false, Cuisine = "Indian", Rating = 4.6, Image = "butter_chicken.jpg" },
            new() { RestaurantName = "Sugar Rush", ItemName = "Chocolate Lava Cake", Description = "Warm molten core, vanilla ice cream", Price = 12.00m, IsVeg = true, Cuisine = "Dessert", Rating = 4.9, Image = "cake.jpg" },
        };

        await _database.InsertAllAsync(items);
    }

    public async Task<T?> GetByIdAsync<T>(int id) where T : class, new()
    {
        if (_database == null) return null;
        return await _database.GetAsync<T>(id);
    }

    public async Task<List<T>> GetAllAsync<T>() where T : class, new()
    {
        if (_database == null) return new List<T>();
        return await _database.Table<T>().ToListAsync();
    }

    public async Task<List<T>> QueryAsync<T>(string sql, params object[] parameters) where T : class, new()
    {
        if (_database == null) return new List<T>();
        return await _database.QueryAsync<T>(sql, parameters);
    }

    public async Task<int> InsertAsync<T>(T item) where T : class, new()
    {
        if (_database == null) return 0;
        return await _database.InsertAsync(item);
    }

    public async Task<int> InsertAllAsync<T>(IEnumerable<T> items) where T : class, new()
    {
        if (_database == null) return 0;
        return await _database.InsertAllAsync(items);
    }

    public async Task<int> UpdateAsync<T>(T item) where T : class, new()
    {
        if (_database == null) return 0;
        return await _database.UpdateAsync(item);
    }

    public async Task<int> DeleteAsync<T>(T item) where T : class, new()
    {
        if (_database == null) return 0;
        return await _database.DeleteAsync(item);
    }

    public async Task DeleteAsync<T>(int id) where T : class, new()
    {
        if (_database == null) return;

        var entity = await _database.FindAsync<T>(id);
        if (entity != null)
        {
            await _database.DeleteAsync(entity);
        }
    }

    public async Task<int> DeleteAllAsync<T>() where T : class, new()
    {
        if (_database == null) return 0;
        return await _database.DeleteAllAsync<T>();
    }

    public async Task<bool> ExecuteTransactionAsync(Func<Task> action)
    {
        if (_database == null) return false;

        try
        {
            await Task.Run(() =>
            {
                _database.RunInTransactionAsync(conn =>
                {
                    action().GetAwaiter().GetResult();
                });
            });
            return true;
        }
        catch
        {
            return false;
        }
    }


}
