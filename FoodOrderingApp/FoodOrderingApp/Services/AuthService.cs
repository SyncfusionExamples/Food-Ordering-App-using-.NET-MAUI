using System.Text.RegularExpressions;
using FoodOrderingApp.Models;

namespace FoodOrderingApp.Services;

public class AuthService : IAuthService
{
    private readonly IDatabaseService _databaseService;
    private readonly IValidationService _validationService;
    private const string SessionKeyUserId = "session_userid";
    private const string SessionKeyEmail = "session_email";
    
    private string? _cachedUserIdStr;
    private string? _cachedEmail;
    private bool _sessionCacheLoaded = false;

    public AuthService(IDatabaseService databaseService, IValidationService validationService)
    {
        _databaseService = databaseService;
        _validationService = validationService;
    }
    
    private async Task EnsureSessionCacheLoadedAsync()
    {
        if (!_sessionCacheLoaded)
        {
            try
            {
                _cachedUserIdStr = await SecureStorage.GetAsync(SessionKeyUserId);
                _cachedEmail = await SecureStorage.GetAsync(SessionKeyEmail);
            }
            catch
            {
                // Ignore errors, cache will be empty
            }
            finally
            {
                _sessionCacheLoaded = true;
            }
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new AuthResult
            {
                IsSuccessful = false,
                ErrorMessage = "Email and password are required"
            };
        }

        try
        {
            var users = await _databaseService.QueryAsync<User>(
                "SELECT * FROM Users WHERE LOWER(Email) = LOWER(?)", email?.ToLower() ?? string.Empty);

            var user = users.FirstOrDefault();
            if (user == null)
            {
                return new AuthResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Invalid email or password"
                };
            }

            bool passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!passwordValid)
            {
                return new AuthResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Invalid email or password"
                };
            }

            await SecureStorage.SetAsync(SessionKeyUserId, user.UserId.ToString());
            await SecureStorage.SetAsync(SessionKeyEmail, user.Email);

            _cachedUserIdStr = user.UserId.ToString();
            _cachedEmail = user.Email;
            _sessionCacheLoaded = true;


            return new AuthResult
            {
                IsSuccessful = true,
                User = user
            };
        }
        catch (Exception ex)
        {
            return new AuthResult
            {
                IsSuccessful = false,
                ErrorMessage = $"Login failed: {ex.Message}"
            };
        }
    }

    public async Task<AuthResult> SignUpAsync(string fullName, string email, string password)
    {
        var (nameValid, nameError) = _validationService.ValidateRequired(fullName, "Full name");
        if (!nameValid)
        {
            return new AuthResult
            {
                IsSuccessful = false,
                ErrorMessage = nameError
            };
        }

        if (!_validationService.IsValidEmail(email))
        {
            return new AuthResult
            {
                IsSuccessful = false,
                ErrorMessage = "Please enter a valid email address"
            };
        }

        var (passwordValid, passwordError) = _validationService.ValidatePassword(password);
        if (!passwordValid)
        {
            return new AuthResult
            {
                IsSuccessful = false,
                ErrorMessage = passwordError
            };
        }

        try
        {
            var existingUsers = await _databaseService.QueryAsync<User>(
                "SELECT * FROM Users WHERE LOWER(Email) = LOWER(?)", email?.ToLower() ?? string.Empty);

            System.Diagnostics.Debug.WriteLine($"[AuthService.SignUp] Email query for '{email}' returned {existingUsers.Count} users");
            foreach (var user in existingUsers)
            {
                System.Diagnostics.Debug.WriteLine($"  - Found user: {user.Email}");
            }

            if (existingUsers.Any())
            {
                return new AuthResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Email already registered"
                };
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = passwordHash,
                JoinDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _databaseService.InsertAsync(newUser);

            return new AuthResult
            {
                IsSuccessful = true,
                User = newUser
            };
        }
        catch (Exception ex)
        {
            return new AuthResult
            {
                IsSuccessful = false,
                ErrorMessage = $"Signup failed: {ex.Message}"
            };
        }
    }

    public async Task<AuthResult> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(oldPassword))
        {
            return new AuthResult
            {
                IsSuccessful = false,
                ErrorMessage = "Current password is required"
            };
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            return new AuthResult
            {
                IsSuccessful = false,
                ErrorMessage = "New password must be at least 8 characters long"
            };
        }

        try
        {
            var user = await _databaseService.GetByIdAsync<User>(userId);
            if (user == null)
            {
                return new AuthResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "User not found"
                };
            }

            bool passwordValid = BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash);
            if (!passwordValid)
            {
                return new AuthResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Current password is incorrect"
                };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _databaseService.UpdateAsync(user);

            return new AuthResult
            {
                IsSuccessful = true,
                User = user
            };
        }
        catch (Exception ex)
        {
            return new AuthResult
            {
                IsSuccessful = false,
                ErrorMessage = $"Password change failed: {ex.Message}"
            };
        }
    }

    public bool IsSessionValid()
    {
        return !string.IsNullOrEmpty(_cachedUserIdStr) && !string.IsNullOrEmpty(_cachedEmail);
    }

    public async Task<bool> IsSessionValidAsync()
    {
        try
        {
            var userIdStr = await SecureStorage.GetAsync(SessionKeyUserId);
            var email = await SecureStorage.GetAsync(SessionKeyEmail);

            return !string.IsNullOrEmpty(userIdStr) && !string.IsNullOrEmpty(email);
        }
        catch
        {
            return false;
        }
    }

    public void ClearSession()
    {
        try
        {
            SecureStorage.RemoveAll();
        }
        catch
        {
            // Ignore errors during logout
        }
        
        _cachedUserIdStr = null;
        _cachedEmail = null;
        _sessionCacheLoaded = false;
    }

    public async Task LogoutAsync()
    {
        ClearSession();
        await Task.CompletedTask;
    }

    public async Task<bool> ValidatePasswordAsync(string email, string password)
    {
        try
        {
            var users = await _databaseService.QueryAsync<User>(
                "SELECT * FROM Users WHERE Email = ?", email);

            var user = users.FirstOrDefault();
            if (user == null)
                return false;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(string email, string newPassword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                return false;

            var users = await _databaseService.QueryAsync<User>(
                "SELECT * FROM Users WHERE Email = ?", email);

            var user = users.FirstOrDefault();
            if (user == null)
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _databaseService.UpdateAsync(user);
            
            _sessionCacheLoaded = false;
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public int? GetCurrentUserId()
    {
        if (int.TryParse(_cachedUserIdStr, out var userId))
        {
            return userId;
        }
        return null;
    }

    public string? GetCurrentUserEmail()
    {
        return _cachedEmail;
    }
    
    public async Task<int?> GetCurrentUserIdAsync()
    {
        await EnsureSessionCacheLoadedAsync();
        return GetCurrentUserId();
    }

    public async Task<string?> GetCurrentUserEmailAsync()
    {
        await EnsureSessionCacheLoadedAsync();
        return GetCurrentUserEmail();
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return null;

        return await _databaseService.GetByIdAsync<User>(userId.Value);
    }

    public async Task<bool> UpdateProfileAsync(int userId, string fullName, string email, DateTime dob)
    {
        try
        {
            var user = await _databaseService.GetByIdAsync<User>(userId);
            if (user == null)
                return false;

            user.FullName = fullName;
            user.Email = email;
            user.UpdatedAt = DateTime.UtcNow;

            await _databaseService.UpdateAsync(user);
            
            if (GetCurrentUserId() == userId)
            {
                _cachedEmail = email;
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Address>> GetUserAddressesAsync(int userId)
    {
        try
        {
            return await _databaseService.QueryAsync<Address>(
                "SELECT * FROM Addresses WHERE UserId = ? ORDER BY IsDefault DESC, AddressId DESC", userId);
        }
        catch
        {
            return new List<Address>();
        }
    }

    public async Task<Address?> GetAddressAsync(int addressId)
    {
        try
        {
            return await _databaseService.GetByIdAsync<Address>(addressId);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> AddAddressAsync(Address address)
    {
        try
        {
            address.CreatedAt = DateTime.UtcNow;
            address.UpdatedAt = DateTime.UtcNow;

            await _databaseService.InsertAsync(address);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateAddressAsync(Address address)
    {
        try
        {
            address.UpdatedAt = DateTime.UtcNow;
            await _databaseService.UpdateAsync(address);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAddressAsync(int addressId)
    {
        try
        {
            await _databaseService.DeleteAsync<Address>(addressId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
    {
        try
        {
            var addresses = await _databaseService.QueryAsync<Address>(
                "SELECT * FROM Addresses WHERE UserId = ?", userId);

            await _databaseService.ExecuteTransactionAsync(async () =>
            {
                foreach (var addr in addresses)
                {
                    addr.IsDefault = (addr.AddressId == addressId);
                    addr.UpdatedAt = DateTime.UtcNow;
                    await _databaseService.UpdateAsync(addr);
                }
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }
        catch
        {
            return false;
        }
    }
}
