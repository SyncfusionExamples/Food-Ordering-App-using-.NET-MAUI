using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodOrderingApp.Models;

namespace FoodOrderingApp.Services;

public class PaymentService : IPaymentService
{
    private readonly Random _random = new Random();

    public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentMethod method)
    {
        // Simulate payment processing with 2-3 second delay
        await Task.Delay(_random.Next(2000, 3500));

        // Simulate random failure (15% chance of failure for testing)
        if (_random.Next(0, 100) < 15)
        {
            return new PaymentResult
            {
                IsSuccessful = false,
                ErrorMessage = "Payment failed. Please try again.",
                ProcessedAt = DateTime.UtcNow
            };
        }

        // Success
        var transactionId = await GenerateTransactionIdAsync();

        return new PaymentResult
        {
            IsSuccessful = true,
            TransactionId = transactionId,
            ProcessedAt = DateTime.UtcNow
        };
    }

    public async Task<bool> ValidatePaymentMethodAsync(PaymentMethod method)
    {
        // Simulate validation (simulating network call with small delay)
        await Task.Delay(_random.Next(100, 500));

        return method switch
        {
            PaymentMethod.UPI => true,
            PaymentMethod.NetBanking => true,
            PaymentMethod.CreditCard => true,
            PaymentMethod.DebitCard => true,
            _ => false
        };
    }

    public async Task<string> GenerateTransactionIdAsync()
    {
        // Generate unique transaction ID: TXN_TIMESTAMP_RANDOM
        await Task.Delay(50); // Minimal delay

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var randomSuffix = _random.Next(10000, 99999);

        return $"TXN_{timestamp}_{randomSuffix}";
    }
}
