using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodOrderingApp.Models;

namespace FoodOrderingApp.Services;

public enum PaymentMethod
{
    UPI,
    NetBanking,
    CreditCard,
    DebitCard
}

public class PaymentResult
{
    public bool IsSuccessful { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentMethod method);
    Task<bool> ValidatePaymentMethodAsync(PaymentMethod method);
    Task<string> GenerateTransactionIdAsync();
}
