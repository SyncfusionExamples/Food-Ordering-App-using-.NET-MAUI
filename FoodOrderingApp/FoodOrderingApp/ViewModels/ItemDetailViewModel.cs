using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FoodOrderingApp.Models;
using FoodOrderingApp.Services;
using Microsoft.Maui.Controls;

namespace FoodOrderingApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
public class ItemDetailViewModel : INotifyPropertyChanged
{
    private readonly IDatabaseService _databaseService;
    private readonly ICartService? _cartService;
    private int _itemId = 0;
    private Item? _item = null;
    private int _quantity = 1;
    private bool _isLoading = false;
    private string _successMessage = string.Empty;
    private bool _showSuccessMessage = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int ItemId
    {
        get => _itemId;
        set
        {
            if (SetProperty(ref _itemId, value))
            {
                _ = LoadItemAsync();
            }
        }
    }

    public Item? Item
    {
        get => _item;
        set => SetProperty(ref _item, value);
    }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (value >= 1 && value <= 99)
            {
                SetProperty(ref _quantity, value);
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    public bool ShowSuccessMessage
    {
        get => _showSuccessMessage;
        set => SetProperty(ref _showSuccessMessage, value);
    }

    public ICommand IncrementQuantityCommand { get; }
    public ICommand DecrementQuantityCommand { get; }
    public ICommand AddToCartCommand { get; }
    public ICommand CloseCommand { get; }

    public ItemDetailViewModel(IDatabaseService databaseService, ICartService? cartService = null)
    {
        _databaseService = databaseService;
        _cartService = cartService;

        IncrementQuantityCommand = new RelayCommand(IncrementQuantity);
        DecrementQuantityCommand = new RelayCommand(DecrementQuantity);
        AddToCartCommand = new AsyncRelayCommand(AddToCartAsync);
        CloseCommand = new RelayCommand(Close);
    }

    private async Task LoadItemAsync()
    {
        if (ItemId <= 0)
            return;

        IsLoading = true;

        try
        {
            Item = await _databaseService.GetByIdAsync<Item>(ItemId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading item: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void IncrementQuantity()
    {
        if (Quantity < 99)
            Quantity++;
    }

    private void DecrementQuantity()
    {
        if (Quantity > 1)
            Quantity--;
    }

    private async Task AddToCartAsync()
    {
        if (Item == null || _cartService == null)
        {
            ShowSuccessMessage = false;
            return;
        }

        IsLoading = true;

        try
        {
            // Add item to cart
            var result = await _cartService.AddToCartAsync(Item.ItemId, Quantity);

            if (result)
            {
                SuccessMessage = $"Added {Quantity} {Item.ItemName}(s) to cart!";
                ShowSuccessMessage = true;

                // Hide success message after 2 seconds and close popup
                await Task.Delay(500);
                Close();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error adding to cart: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Close()
    {
        Shell.Current?.GoToAsync("..");
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
