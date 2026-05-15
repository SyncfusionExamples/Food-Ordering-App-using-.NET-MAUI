using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FoodOrderingApp.Models;
using FoodOrderingApp.Services;
using Microsoft.Maui.Controls;

namespace FoodOrderingApp.ViewModels;

[QueryProperty(nameof(TotalAmount), "total")]
public class CheckoutViewModel : INotifyPropertyChanged
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private decimal _totalAmount = 0;
    private PaymentMethod _selectedPaymentMethod = PaymentMethod.UPI;
    private bool _isLoading = false;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _showSuccessMessage = false;
    private bool _paymentProcessing = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public decimal TotalAmount
    {
        get => _totalAmount;
        set => SetProperty(ref _totalAmount, value);
    }

    public PaymentMethod SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set => SetProperty(ref _selectedPaymentMethod, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
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

    public bool PaymentProcessing
    {
        get => _paymentProcessing;
        set => SetProperty(ref _paymentProcessing, value);
    }

    public ObservableCollection<PaymentMethodOption> PaymentMethods { get; } = new()
    {
        new PaymentMethodOption { Method = PaymentMethod.UPI, DisplayName = "💳 UPI", Description = "Pay with UPI ID" },
        new PaymentMethodOption { Method = PaymentMethod.NetBanking, DisplayName = "🏦 Net Banking", Description = "Direct bank transfer" },
        new PaymentMethodOption { Method = PaymentMethod.CreditCard, DisplayName = "💰 Credit Card", Description = "Visa, MasterCard, Amex" },
        new PaymentMethodOption { Method = PaymentMethod.DebitCard, DisplayName = "🏧 Debit Card", Description = "All major banks" }
    };

    public ICommand ConfirmPaymentCommand { get; }
    public ICommand CancelCommand { get; }

    public CheckoutViewModel(ICartService cartService, IOrderService orderService, IPaymentService paymentService)
    {
        _cartService = cartService;
        _orderService = orderService;
        _paymentService = paymentService;

        ConfirmPaymentCommand = new AsyncRelayCommand(ConfirmPaymentAsync);
        CancelCommand = new RelayCommand(Cancel);
    }

    private async Task ConfirmPaymentAsync()
    {
        ErrorMessage = string.Empty;
        ShowSuccessMessage = false;

        if (TotalAmount <= 0)
        {
            ErrorMessage = "Invalid amount";
            return;
        }

        // Validate payment method
        IsLoading = true;
        try
        {
            var isValid = await _paymentService.ValidatePaymentMethodAsync(SelectedPaymentMethod);
            if (!isValid)
            {
                ErrorMessage = "Selected payment method is not available";
                return;
            }
        }
        finally
        {
            IsLoading = false;
        }

        // Process payment
        PaymentProcessing = true;
        try
        {
            var paymentResult = await _paymentService.ProcessPaymentAsync(TotalAmount, SelectedPaymentMethod);

            if (paymentResult.IsSuccessful)
            {
                // Create order
                var order = await _orderService.CreateOrderAsync(TotalAmount);

                if (order != null)
                {
                    SuccessMessage = $"Order placed successfully!\nOrder ID: {order.OrderId}\nTransaction ID: {paymentResult.TransactionId}";
                    ShowSuccessMessage = true;

                    // Wait 3 seconds and then navigate to orders page
                    await Task.Delay(3000);
                    await Shell.Current.GoToAsync("//orders");
                }
                else
                {
                    ErrorMessage = "Order creation failed. Please try again.";
                }
            }
            else
            {
                ErrorMessage = paymentResult.ErrorMessage ?? "Payment processing failed. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            PaymentProcessing = false;
        }
    }

    private void Cancel()
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

public class PaymentMethodOption
{
    public PaymentMethod Method { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
