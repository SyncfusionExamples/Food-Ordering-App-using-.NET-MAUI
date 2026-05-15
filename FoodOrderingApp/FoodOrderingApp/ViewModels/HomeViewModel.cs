using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using FoodOrderingApp.Models;
using FoodOrderingApp.Services;
using Microsoft.Maui.Controls;

namespace FoodOrderingApp.ViewModels;

public class HomeViewModel : INotifyPropertyChanged
{
    private readonly IDatabaseService _databaseService;
    private readonly IAuthService _authService;
    private ObservableCollection<Item> _items = new();
    private ObservableCollection<Item> _filteredItems = new();
    private string _searchQuery = string.Empty;
    private bool _showVegetarianOnly = false;
    private bool _isLoading = false;
    private User? _currentUser = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Item> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    public ObservableCollection<Item> FilteredItems
    {
        get => _filteredItems;
        set => SetProperty(ref _filteredItems, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                ApplyFilters();
            }
        }
    }

    public bool ShowVegetarianOnly
    {
        get => _showVegetarianOnly;
        set
        {
            if (SetProperty(ref _showVegetarianOnly, value))
            {
                ApplyFilters();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public User? CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    public ICommand LoadItemsCommand { get; }
    public ICommand ItemSelectedCommand { get; }
    public ICommand LogoutCommand { get; }

    [Obsolete]
    public HomeViewModel(IDatabaseService databaseService, IAuthService authService)
    {
        _databaseService = databaseService;
        _authService = authService;

        LoadItemsCommand = new AsyncRelayCommand(LoadItemsAsync);
        ItemSelectedCommand = new AsyncRelayCommand<Item>(ItemSelectedAsync);
        LogoutCommand = new RelayCommand(Logout);
    }

    public async Task InitializeAsync()
    {
        await LoadItemsAsync();
        await LoadCurrentUserAsync();
    }

    private async Task LoadItemsAsync()
    {
        IsLoading = true;

        try
        {
            var items = await _databaseService.GetAllAsync<Item>();
            Items = new ObservableCollection<Item>(items);
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading items: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadCurrentUserAsync()
    {
        try
        {
            CurrentUser = await _authService.GetCurrentUserAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading current user: {ex.Message}");
        }
    }

    private void ApplyFilters()
    {
        var filtered = Items.AsEnumerable();

        // Filter by search query
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var query = SearchQuery.ToLower();
            filtered = filtered.Where(item =>
                item.ItemName.ToLower().Contains(query) ||
                item.RestaurantName.ToLower().Contains(query) ||
                item.Cuisine.ToLower().Contains(query));
        }

        // Filter by vegetarian
        if (ShowVegetarianOnly)
        {
            filtered = filtered.Where(item => item.IsVeg);
        }

        FilteredItems = new ObservableCollection<Item>(filtered);
    }

    private async Task ItemSelectedAsync(Item? item)
    {
        if (item == null)
            return;

        try
        {
            // Navigate to item detail with the item ID
            await Shell.Current.GoToAsync($"itemdetail?itemId={item.ItemId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error navigating to item detail: {ex.Message}");
        }
    }

    [Obsolete]
    private void Logout()
    {
        _authService.ClearSession();

        if (App.Current?.MainPage is AppShell shell)
        {
            shell.ShowAuthPages();
            Shell.Current.GoToAsync("//");
        }
    }

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
    {
        if (Equals(storage, value))
            return false;

        storage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
