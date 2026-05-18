using CommunityToolkit.Mvvm.Input;
using FoodOrderingApp.Models;
using FoodOrderingApp.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FoodOrderingApp.ViewModels;

public class CartViewModel : INotifyPropertyChanged
{
    private readonly ICartService _cartService;
    private readonly IDatabaseService _databaseService;
    private ObservableCollection<CartItemViewModel> _cartItems = new();
    private decimal _subtotal = 0;
    private decimal _tax = 0.18m; // 18% GST
    private decimal _deliveryFee = 50;
    private decimal _total = 0;
    private bool _isLoading = false;
    private bool _isCartEmpty = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CartItemViewModel> CartItems
    {
        get => _cartItems;
        set => SetProperty(ref _cartItems, value);
    }

    public decimal Subtotal
    {
        get => _subtotal;
        set
        {
            if (SetProperty(ref _subtotal, value))
                CalculateTotal();
        }
    }

    public decimal Tax
    {
        get => _tax;
        set
        {
            SetProperty(ref _tax, value);
            CalculateTotal();
        }
    }

    public decimal DeliveryFee
    {
        get => _deliveryFee;
        set
        {
            SetProperty(ref _deliveryFee, value);
            CalculateTotal();
        }
    }

    public decimal Total
    {
        get => _total;
        set => SetProperty(ref _total, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsCartEmpty
    {
        get => _isCartEmpty;
        set => SetProperty(ref _isCartEmpty, value);
    }

    public ICommand LoadCartCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand CheckoutCommand { get; }
    public ICommand ContinueShoppingCommand { get; }

    public CartViewModel(ICartService cartService, IDatabaseService databaseService)
    {
        _cartService = cartService;
        _databaseService = databaseService;

        LoadCartCommand = new AsyncRelayCommand(LoadCartAsync);
        RemoveItemCommand = new AsyncRelayCommand<CartItemViewModel>(RemoveItemAsync);
        CheckoutCommand = new AsyncRelayCommand(CheckoutAsync);
        ContinueShoppingCommand = new RelayCommand(ContinueShopping);
    }

    public async Task InitializeAsync()
    {
        await LoadCartAsync();
    }

    private async Task LoadCartAsync()
    {
        IsLoading = true;

        try
        {
            var cartItems = await _cartService.GetCartItemsAsync();
            var cartItemViewModels = new List<CartItemViewModel>();

            foreach (var cartItem in cartItems)
            {
                var item = await _databaseService.GetByIdAsync<Item>(cartItem.ItemId);
                if (item != null)
                {
                    cartItemViewModels.Add(new CartItemViewModel
                    {
                        CartItem = cartItem,
                        Item = item,
                        RemoveCommand = RemoveItemCommand
                    });
                }
            }

            CartItems = new ObservableCollection<CartItemViewModel>(cartItemViewModels);
            IsCartEmpty = CartItems.Count == 0;

            Subtotal = await _cartService.CalculateTotalAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading cart: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RemoveItemAsync(CartItemViewModel? cartItemVm)
    {
        if (cartItemVm == null)
            return;

        IsLoading = true;

        try
        {
            var success = await _cartService.RemoveFromCartAsync(cartItemVm.CartItem.CartItemId);
            if (success)
            {
                CartItems.Remove(cartItemVm);
                IsCartEmpty = CartItems.Count == 0;
                Subtotal = await _cartService.CalculateTotalAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error removing item: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CheckoutAsync()
    {
        if (CartItems.Count == 0)
            return;

        try
        {
            var subtotal = await _cartService.CalculateTotalAsync();
            var tax = subtotal * 0.18m;
            var deliveryFee = 50m;
            var total = subtotal + tax + deliveryFee;
            await Shell.Current.GoToAsync("checkout", new Dictionary<string, object>
            {
                { "total", total.ToString("F2", CultureInfo.InvariantCulture) }
            });

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error navigating to checkout: {ex.Message}");
        }
    }

    private void ContinueShopping()
    {
        Shell.Current?.GoToAsync("//home");
    }

    private void CalculateTotal()
    {
        var taxAmount = Subtotal * Tax;
        Total = Subtotal + taxAmount + DeliveryFee;
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

public class CartItemViewModel
{
    public CartItem CartItem { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public ICommand RemoveCommand { get; set; } = null!;

    public string DisplayName => Item.ItemName;
    public string RestaurantName => Item.RestaurantName;
    public decimal UnitPrice => Item.Price;
    public int Quantity => CartItem.Quantity;
    public decimal Total => UnitPrice * Quantity;
}
