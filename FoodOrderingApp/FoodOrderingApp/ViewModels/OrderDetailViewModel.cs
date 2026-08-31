using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FoodOrderingApp.Models;
using FoodOrderingApp.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace FoodOrderingApp.ViewModels;

[QueryProperty(nameof(OrderId), "id")]
public class OrderDetailViewModel : INotifyPropertyChanged
{
    private readonly IOrderService _orderService;
    private readonly IMapService _mapService;
    private readonly Random _random = new Random();
    private int _orderId = 0;
    private bool _isLoading = false;
    private Order? _order;
    private string _orderIdDisplay = string.Empty;
    private string _formattedDate = string.Empty;
    private string _formattedAmount = string.Empty;
    private string _statusDisplayName = string.Empty;
    private string _statusColor = string.Empty;
    private string _restaurantName = "Your Restaurant";
    private string _deliveryAddress = "Loading...";
    private string _estimatedDeliveryTime = "Calculating...";
    private bool _canCancelOrder = false;
    private string _deliveryPartnerName = "Finding partner...";
    private string _deliveryPartnerPhone = "";
    private string _deliveryPartnerVehicle = "";
    private string _deliveryPartnerRating = "";
    private double _deliveryPartnerLatitude = 0;
    private double _deliveryPartnerLongitude = 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<TimelineItem> Timeline { get; } = new();
    public ObservableCollection<OrderItemDetail> OrderItems { get; } = new();

    public int OrderId
    {
        get => _orderId;
        set
        {
            if (SetProperty(ref _orderId, value))
            {
                // Load order details when ID is set
                _ = LoadOrderAsync();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public Order? Order
    {
        get => _order;
        set => SetProperty(ref _order, value);
    }

    public string OrderIdDisplay
    {
        get => _orderIdDisplay;
        set => SetProperty(ref _orderIdDisplay, value);
    }

    public string FormattedDate
    {
        get => _formattedDate;
        set => SetProperty(ref _formattedDate, value);
    }

    public string FormattedAmount
    {
        get => _formattedAmount;
        set => SetProperty(ref _formattedAmount, value);
    }

    public string StatusDisplayName
    {
        get => _statusDisplayName;
        set => SetProperty(ref _statusDisplayName, value);
    }

    public string StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }

    public string RestaurantName
    {
        get => _restaurantName;
        set => SetProperty(ref _restaurantName, value);
    }

    public string DeliveryAddress
    {
        get => _deliveryAddress;
        set => SetProperty(ref _deliveryAddress, value);
    }

    public string EstimatedDeliveryTime
    {
        get => _estimatedDeliveryTime;
        set => SetProperty(ref _estimatedDeliveryTime, value);
    }

    public bool CanCancelOrder
    {
        get => _canCancelOrder;
        set => SetProperty(ref _canCancelOrder, value);
    }

    public string DeliveryPartnerName
    {
        get => _deliveryPartnerName;
        set => SetProperty(ref _deliveryPartnerName, value);
    }

    public string DeliveryPartnerPhone
    {
        get => _deliveryPartnerPhone;
        set => SetProperty(ref _deliveryPartnerPhone, value);
    }

    public string DeliveryPartnerVehicle
    {
        get => _deliveryPartnerVehicle;
        set => SetProperty(ref _deliveryPartnerVehicle, value);
    }

    public string DeliveryPartnerRating
    {
        get => _deliveryPartnerRating;
        set => SetProperty(ref _deliveryPartnerRating, value);
    }

    public double DeliveryPartnerLatitude
    {
        get => _deliveryPartnerLatitude;
        set => SetProperty(ref _deliveryPartnerLatitude, value);
    }

    public double DeliveryPartnerLongitude
    {
        get => _deliveryPartnerLongitude;
        set => SetProperty(ref _deliveryPartnerLongitude, value);
    }

    public ICommand CancelOrderCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand RefreshLocationCommand { get; }

    private readonly List<(string Name, string Phone, string Vehicle)> _samplePartners = new()
    {
        ("Arun Kumar", "98765XXXXX", "Bike TN-09 AB12**"),
        ("Priya Sharma", "91234XXXXX", "Scooter TN-22 XY56**"),
        ("Ravi Menon", "99887XXXXX", "Car TN-05 CD43**"),
        ("Sneha Reddy", "98765XXXXX", "Bike TN-11 EF67**"),
        ("Karthik Iyer", "90012XXXXX", "Scooter TN-07 GH98**")
    };

    public OrderDetailViewModel(IOrderService orderService, IMapService mapService)
    {
        _orderService = orderService;
        _mapService = mapService;
        CancelOrderCommand = new AsyncRelayCommand(CancelOrderAsync);
        BackCommand = new RelayCommand(Back);
        RefreshLocationCommand = new AsyncRelayCommand(RefreshLocationAsync);
        AssignRandomPartner();
    }

    private void AssignRandomPartner()
    {
        var partner = _samplePartners[_random.Next(_samplePartners.Count)];

        DeliveryPartnerName = partner.Name;
        DeliveryPartnerPhone = $" {partner.Phone}";
        DeliveryPartnerVehicle = $" {partner.Vehicle}";
        DeliveryPartnerRating = $"⭐ {_random.Next(3, 5)}.{_random.Next(0, 9)}/5.0 ({_random.Next(50, 500)} deliveries)";
    }

    private async Task LoadOrderAsync()
    {
        if (_orderId <= 0) return;

        IsLoading = true;
        try
        {
            var order = await _orderService.GetOrderByIdAsync(_orderId);

            if (order != null)
            {
                Order = order;
                OrderIdDisplay = $"Order #{order.OrderId}";
                FormattedDate = order.OrderDate.ToString("dddd, MMMM dd, yyyy 'at' HH:mm");
                FormattedAmount = $"₹{order.TotalAmount:F2}";
                StatusDisplayName = GetStatusDisplayName(order.Status);
                StatusColor = GetStatusColor(order.Status);

                // Load order items
                await LoadOrderItemsAsync(order);

                // Build timeline
                BuildTimeline(order.Status);

                // Set cancel availability (only if Confirmed)
                CanCancelOrder = order.Status == "Confirmed";

                // Load delivery partner info if order is being prepared or delivered
                if (order.Status != "Confirmed" && order.Status != "Cancelled")
                {
                    await LoadDeliveryPartnerInfoAsync(_orderId);
                    await RefreshLocationAsync();
                }

                // Calculate estimated delivery time
                CalculateEstimatedDeliveryTime(order);
            }
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", $"Failed to load order: {ex.Message}", "OK");
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadOrderItemsAsync(Order order)
    {
        // This would load OrderItems from database in real implementation
        // For now, adding placeholder data
        OrderItems.Clear();

        // In a real app, you'd fetch OrderItems from the database
        // For demonstration, we'll show that items would be loaded here
        OrderItems.Add(new OrderItemDetail
        {
            ItemName = "Loading items...",
            Quantity = 1,
            UnitPrice = order.TotalAmount,
            FormattedPrice = $"₹{order.TotalAmount:F2}"
        });
    }

    private void BuildTimeline(string status)
    {
        Timeline.Clear();

        var stages = new[]
        {
            new { Stage = "Confirmed", DisplayName = "✓ Order Confirmed", IsCompleted = true, Icon = "📋" },
            new { Stage = "Preparing", DisplayName = "🍳 Preparing", IsCompleted = status == "Preparing" || status == "OutForDelivery" || status == "Delivered", Icon = "👨‍🍳" },
            new { Stage = "OutForDelivery", DisplayName = "🚗 Out for Delivery", IsCompleted = status == "OutForDelivery" || status == "Delivered", Icon = "🚗" },
            new { Stage = "Delivered", DisplayName = "✅ Delivered", IsCompleted = status == "Delivered", Icon = "📦" }
        };

        foreach (var stage in stages)
        {
            var isCurrent = stage.Stage == status;

            Timeline.Add(new TimelineItem
            {
                Stage = stage.Stage,
                DisplayName = stage.DisplayName,
                IsCompleted = stage.IsCompleted,
                IsCurrent = isCurrent,
                Icon = stage.Icon,
                TextColor = GetTimelineTextColor(stage.IsCompleted, isCurrent),
                LineColor = stage.IsCompleted ? "#16A34A" : "#E0E0E0"
            });
        }
    }

    private static string GetTimelineTextColor(bool isCompleted, bool isCurrent)
    {
        if (isCurrent) return "#FF6B35";  // Primary orange
        if (isCompleted) return "#16A34A"; // Green
        return "#999999";                 // Gray
    }

    private void CalculateEstimatedDeliveryTime(Order order)
    {
        // Simple calculation: assume 30 min from order, varies by status
        var estimatedMinutes = order.Status switch
        {
            "Confirmed" => 30,
            "Preparing" => 20,
            "OutForDelivery" => 10,
            "Delivered" => 0,
            _ => 30
        };

        if (estimatedMinutes > 0)
        {
            var deliveryTime = order.OrderDate.AddMinutes(estimatedMinutes);
            EstimatedDeliveryTime = $"Expected by {deliveryTime:HH:mm}";
        }
        else
        {
            EstimatedDeliveryTime = "Delivered";
        }
    }

    [Obsolete]
    private async Task CancelOrderAsync()
    {
        if (Order == null) return;

        bool result = false;
        if (Application.Current?.MainPage != null)
        {
            result = await Application.Current.MainPage.DisplayAlert(
                "Cancel Order",
                "Are you sure you want to cancel this order?",
                "Yes",
                "No");
        }

        if (!result) return;

        IsLoading = true;
        try
        {
            await _orderService.CancelOrderAsync(Order.OrderId);
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current?.MainPage?.DisplayAlert("Success", "Order cancelled successfully", "OK");
                await Shell.Current.GoToAsync("//orders");
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", $"Failed to cancel order: {ex.Message}", "OK");
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadDeliveryPartnerInfoAsync(int orderId)
    {
        try
        {
            var partner = await _mapService.GetDeliveryPartnerAsync(orderId);
            if (partner != null)
            {
                DeliveryPartnerName = partner.Name;
                DeliveryPartnerPhone = $"📞 {partner.PhoneNumber}";
                DeliveryPartnerVehicle = $"🚗 {partner.VehicleType} ({partner.VehicleNumber})";
                DeliveryPartnerRating = $"⭐ {partner.Rating}/5.0 ({partner.TotalDeliveries} deliveries)";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading delivery partner: {ex.Message}");
        }
    }

    private async Task RefreshLocationAsync()
    {
        try
        {
            var locationUpdate = await _mapService.GetLocationUpdateAsync(_orderId);
            if (locationUpdate != null)
            {
                DeliveryPartnerLatitude = locationUpdate.Latitude;
                DeliveryPartnerLongitude = locationUpdate.Longitude;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error refreshing location: {ex.Message}");
        }
    }

    private void Back()
    {
        Shell.Current?.GoToAsync("//orders");
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

public class TimelineItem
{
    public string Stage { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string TextColor { get; set; } = string.Empty;
    public string LineColor { get; set; } = string.Empty;
}

public class OrderItemDetail
{
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string FormattedPrice { get; set; } = string.Empty;
}
