using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using FoodOrderingApp.Models;
using FoodOrderingApp.Services;
using Microsoft.Maui.Controls;

namespace FoodOrderingApp.ViewModels;

public class OrdersViewModel : INotifyPropertyChanged
{
    private readonly IOrderService _orderService;
    private readonly IAuthService _authService;
    private bool _isLoading = false;
    private string _emptyMessage = "No orders yet. Start ordering!";

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<OrderViewModel> Orders { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string EmptyMessage
    {
        get => _emptyMessage;
        set => SetProperty(ref _emptyMessage, value);
    }

    public ICommand LoadOrdersCommand { get; }
    public ICommand OrderSelectedCommand { get; }

    public OrdersViewModel(IOrderService orderService, IAuthService authService)
    {
        _orderService = orderService;
        _authService = authService;

        LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync);
        OrderSelectedCommand = new AsyncRelayCommand<OrderViewModel>(OrderSelectedAsync);
        Orders.CollectionChanged += (s, e) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Orders)));
    }

    public async Task InitializeAsync()
    {
        await LoadOrdersAsync();
    }

    private async Task LoadOrdersAsync()
    {
        IsLoading = true;
        try
        {
            Orders.Clear();

            var orders = await _orderService.GetUserOrdersAsync();

            if (orders == null || orders.Count == 0)
            {
                EmptyMessage = "No orders yet. Start ordering!";
                return;
            }

            foreach (var order in orders)
            {
                Orders.Add(new OrderViewModel
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    StatusDisplayName = GetStatusDisplayName(order.Status),
                    StatusColor = GetStatusColor(order.Status),
                    FormattedDate = order.OrderDate.ToString("MMM dd, yyyy 'at' HH:mm"),
                    FormattedAmount = $"₹{order.TotalAmount:F2}"
                });
            }
        }
        catch (Exception ex)
        {
            EmptyMessage = $"Error loading orders: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OrderSelectedAsync(OrderViewModel? order)
    {
        if (order == null) return;

        // Navigate to order detail page with order ID as query parameter
        await Shell.Current.GoToAsync($"orderdetail?id={order.OrderId}");
    }

    private string GetStatusDisplayName(string status)
    {
        return status switch
        {
            "Confirmed" => "✓ Confirmed",
            "Preparing" => "🍳 Preparing",
            "OutForDelivery" => "🚗 Out for Delivery",
            "Delivered" => "✅ Delivered",
            _ => status
        };
    }

    private string GetStatusColor(string status)
    {
        return status switch
        {
            "Confirmed" => "#FF6B35",      // Primary orange
            "Preparing" => "#F7B801",      // Tertiary yellow
            "OutForDelivery" => "#004E89",  // Secondary blue
            "Delivered" => "#16A34A",       // Green
            _ => "#999999"
        };
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

public class OrderViewModel
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusDisplayName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public string FormattedDate { get; set; } = string.Empty;
    public string FormattedAmount { get; set; } = string.Empty;
}
