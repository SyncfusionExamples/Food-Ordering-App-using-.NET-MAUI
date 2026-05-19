using FoodOrderingApp.Models;
using FoodOrderingApp.Services;
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

        await _database.ExecuteAsync("PRAGMA foreign_keys = ON");

        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<Item>();
        await _database.CreateTableAsync<CartItem>();
        await _database.CreateTableAsync<Order>();
        await _database.CreateTableAsync<OrderItem>();
        await _database.CreateTableAsync<Address>();

        await CreateIndexesAsync();

        await NormalizeImageExtensionsAsync();
        var itemCount = await _database.Table<Item>().CountAsync();
        if (itemCount == 0)
        {
            await SeedDataAsync();
        }
    }

    private async Task NormalizeImageExtensionsAsync()
    {
        if (_database == null) return;

        try
        {
            await _database.ExecuteAsync("UPDATE Items SET Image = REPLACE(Image, '.jpg', '.png') WHERE Image LIKE '%.jpg'");
            await _database.ExecuteAsync("UPDATE Items SET Image = REPLACE(Image, '.jpeg', '.png') WHERE Image LIKE '%.jpeg'");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"NormalizeImageExtensionsAsync error: {ex.Message}");
        }
    }

    private async Task CreateIndexesAsync()
    {
        if (_database == null) return;

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
            new() { RestaurantName = "The Burger Loft", ItemName = "Classic Burger", Description = "Juicy beef patty with fresh veggies", Price = 299.50m, IsVeg = false, Cuisine = "American", Rating = 4.8, Image = "burger.png" },
            new() { RestaurantName = "Pizzeria Artisan", ItemName = "Margherita Pizza", Description = "Fresh mozzarella, basil, tomato", Price = 399.99m, IsVeg = true, Cuisine = "Italian", Rating = 4.6, Image = "pizza.png" },
            new() { RestaurantName = "Sakura Sushi Bar", ItemName = "Salmon Poke Bowl", Description = "Brown rice, sesame dressing, avocado", Price = 549.25m, IsVeg = false, Cuisine = "Japanese", Rating = 4.9, Image = "sushi.png" },
            new() { RestaurantName = "Noodle Theory", ItemName = "Pad Thai", Description = "Rice noodles with peanut sauce", Price = 449.50m, IsVeg = true, Cuisine = "Thai", Rating = 4.5, Image = "noodles.png" },
            new() { RestaurantName = "Green Garden", ItemName = "Buddha Bowl", Description = "Mixed veggies, quinoa, tahini dressing", Price = 349.99m, IsVeg = true, Cuisine = "Healthy", Rating = 4.7, Image = "bowl.png" },
            new() { RestaurantName = "Smoke & Fire BBQ", ItemName = "Pulled Pork Sandwich", Description = "Slow-smoked pork with coleslaw", Price = 399.50m, IsVeg = false, Cuisine = "American", Rating = 4.4, Image = "bbq.png" },
            new() { RestaurantName = "Indus Spice", ItemName = "Butter Chicken", Description = "Tender chicken in creamy tomato sauce", Price = 449.99m, IsVeg = false, Cuisine = "Indian", Rating = 4.6, Image = "butter_chicken.png" },
            new() { RestaurantName = "Sugar Rush", ItemName = "Chocolate Lava Cake", Description = "Warm molten core, vanilla ice cream", Price = 249.00m, IsVeg = true, Cuisine = "Dessert", Rating = 4.9, Image = "cake.png" },
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
            await _database.RunInTransactionAsync(async (conn) =>
            {
                await action();
            });
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Transaction error: {ex.Message}");
            return false;
        }
    }

    public async Task<int> ClearUsersAsync()
    {
        if (_database == null) return 0;
        return await _database.DeleteAsync<User>("1=1");
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        if (_database == null) return new List<User>();
        return await _database.Table<User>().ToListAsync();
    }
}
