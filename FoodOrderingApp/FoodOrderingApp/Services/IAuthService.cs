using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodOrderingApp.Models;

namespace FoodOrderingApp.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password);

    Task<AuthResult> SignUpAsync(string fullName, string email, string password);

    Task<AuthResult> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

    Task<bool> ChangePasswordAsync(string email, string newPassword);

    Task<bool> ValidatePasswordAsync(string email, string password);

    Task<bool> UpdateProfileAsync(int userId, string fullName, string email, DateTime dob);

    Task<List<Address>> GetUserAddressesAsync(int userId);

    Task<Address?> GetAddressAsync(int addressId);

    Task<bool> AddAddressAsync(Address address);

    Task<bool> UpdateAddressAsync(Address address);

    Task<bool> DeleteAddressAsync(int addressId);

    Task<bool> SetDefaultAddressAsync(int userId, int addressId);

    bool IsSessionValid();

    Task<bool> IsSessionValidAsync();

    void ClearSession();

    Task LogoutAsync();

    int? GetCurrentUserId();

    string? GetCurrentUserEmail();
    
    Task<int?> GetCurrentUserIdAsync();

    Task<string?> GetCurrentUserEmailAsync();

    Task<User?> GetCurrentUserAsync();
}

public class AuthResult
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public User? User { get; set; }
}
