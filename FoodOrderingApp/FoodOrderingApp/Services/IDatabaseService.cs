using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodOrderingApp.Models;

namespace FoodOrderingApp.Services;

public interface IDatabaseService
{
    Task InitializeAsync();

    Task<T?> GetByIdAsync<T>(int id) where T : class, new();

    Task<List<T>> GetAllAsync<T>() where T : class, new();

    Task<List<T>> QueryAsync<T>(string sql, params object[] parameters) where T : class, new();

    Task<int> InsertAsync<T>(T item) where T : class, new();

    Task<int> InsertAllAsync<T>(IEnumerable<T> items) where T : class, new();

    Task<int> UpdateAsync<T>(T item) where T : class, new();

    Task<int> DeleteAsync<T>(T item) where T : class, new();

    Task<int> DeleteAllAsync<T>() where T : class, new();

    Task DeleteAsync<T>(int id) where T : class, new();

    Task<bool> ExecuteTransactionAsync(Func<Task> action);

    Task<int> ClearUsersAsync();

    Task<List<User>> GetAllUsersAsync();
}
