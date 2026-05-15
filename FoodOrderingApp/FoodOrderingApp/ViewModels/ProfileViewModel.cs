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

[QueryProperty(nameof(AddressId), "id")]
public class ProfileViewModel : INotifyPropertyChanged
{
    private readonly IAuthService _authService;
    private readonly IDatabaseService _databaseService;
    private User? _currentUser;
    private User? _editUser;
    private bool _isEditMode = false;
    private bool _isLoading = false;
    private bool _isChangingPassword = false;
    private string _fullName = string.Empty;
    private string _email = string.Empty;
    private string _joinDate = string.Empty;
    private string _totalOrders = "0";
    private string _rewardsPoints = "0";
    private string _oldPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _showSuccessMessage = false;
    private AddressItem? _currentAddress = null;
    private bool _isSaving = false;
    private int? _editingAddressId = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<AddressItem> Addresses { get; } = new();

    public AddressItem? CurrentAddress
    {
        get => _currentAddress;
        set => SetProperty(ref _currentAddress, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }

    public User? CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsChangingPassword
    {
        get => _isChangingPassword;
        set => SetProperty(ref _isChangingPassword, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string JoinDate
    {
        get => _joinDate;
        set => SetProperty(ref _joinDate, value);
    }

    public string TotalOrders
    {
        get => _totalOrders;
        set => SetProperty(ref _totalOrders, value);
    }

    public string RewardsPoints
    {
        get => _rewardsPoints;
        set => SetProperty(ref _rewardsPoints, value);
    }

    public string OldPassword
    {
        get => _oldPassword;
        set => SetProperty(ref _oldPassword, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
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

    public AsyncRelayCommand SaveProfileCommand { get; }
    public RelayCommand DiscardChangesCommand { get; }
    public AsyncRelayCommand ChangePasswordCommand { get; }
    public AsyncRelayCommand LogoutCommand { get; }
    public AsyncRelayCommand AddAddressCommand { get; }
    public AsyncRelayCommand<AddressItem> EditAddressCommand { get; }
    public AsyncRelayCommand<AddressItem> DeleteAddressCommand { get; }
    public AsyncRelayCommand<AddressItem> SetDefaultAddressCommand { get; }
    public AsyncRelayCommand SaveAddressCommand { get; }
    public AsyncRelayCommand CancelCommand { get; }

    [Obsolete]
    public ProfileViewModel(IAuthService authService, IDatabaseService databaseService)
    {
        _authService = authService;
        _databaseService = databaseService;

        SaveProfileCommand = new AsyncRelayCommand(SaveProfileAsync);
        DiscardChangesCommand = new RelayCommand(DiscardChanges);
        ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync);
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);
        AddAddressCommand = new AsyncRelayCommand(AddAddressAsync);
        EditAddressCommand = new AsyncRelayCommand<AddressItem>(EditAddressAsync);
        DeleteAddressCommand = new AsyncRelayCommand<AddressItem>(DeleteAddressAsync);
        SetDefaultAddressCommand = new AsyncRelayCommand<AddressItem>(SetDefaultAddressAsync);
        SaveAddressCommand = new AsyncRelayCommand(SaveAddressAsync);
        CancelCommand = new AsyncRelayCommand(CancelAddressFormAsync);
    }

    public async Task InitializeAsync()
    {
        await LoadProfileAsync();
    }

    public int AddressId
    {
        set
        {
            if (value > 0)
            {
                // Load address for editing
                _ = LoadAddressForEditAsync(value);
            }
            else
            {
                // New address
                CurrentAddress = new AddressItem();
            }
        }
    }

    private async Task LoadAddressForEditAsync(int addressId)
    {
        try
        {
            var address = await _authService.GetAddressAsync(addressId);
            if (address != null)
            {
                CurrentAddress = new AddressItem
                {
                    AddressId = address.AddressId,
                    Label = address.Label ?? "Home",
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    IsDefault = address.IsDefault
                };
                _editingAddressId = addressId;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading address: {ex.Message}";
        }
    }

    public async Task LoadProfileAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            CurrentUser = await _authService.GetCurrentUserAsync();
            if (CurrentUser != null)
            {
                FullName = CurrentUser.FullName;
                Email = CurrentUser.Email;
                JoinDate = CurrentUser.CreatedAt.ToString("MMMM dd, yyyy");
                RewardsPoints = CurrentUser.RewardsPoints.ToString();

                // Load total orders count
                var orders = await _databaseService.QueryAsync<Order>(
                    "SELECT COUNT(*) as OrderId FROM Orders WHERE UserId = ? AND Status != 'Cancelled'",
                    CurrentUser.UserId);
                TotalOrders = orders.Count.ToString();

                // Load addresses
                await LoadAddressesAsync();

                // Create edit copy
                _editUser = new User
                {
                    UserId = CurrentUser.UserId,
                    FullName = CurrentUser.FullName,
                    Email = CurrentUser.Email,
                    PasswordHash = CurrentUser.PasswordHash,
                    RewardsPoints = CurrentUser.RewardsPoints,
                    CreatedAt = CurrentUser.CreatedAt,
                    UpdatedAt = CurrentUser.UpdatedAt
                };
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading profile: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadAddressesAsync()
    {
        try
        {
            if (CurrentUser == null) return;

            Addresses.Clear();
            var addresses = await _databaseService.QueryAsync<Address>(
                "SELECT * FROM Addresses WHERE UserId = ? ORDER BY IsDefault DESC, AddressId DESC",
                CurrentUser.UserId);

            foreach (var address in addresses)
            {
                Addresses.Add(new AddressItem
                {
                    AddressId = address.AddressId,
                    AddressLine = $"{address.AddressLine1}, {address.City}",
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    IsDefault = address.IsDefault,
                    Label = address.Label ?? "Home"
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading addresses: {ex.Message}";
        }
    }

    private void EditProfile()
    {
        IsEditMode = true;
        _editUser = new User
        {
            UserId = CurrentUser?.UserId ?? 0,
            FullName = FullName,
            Email = Email,
            PasswordHash = CurrentUser?.PasswordHash ?? string.Empty,
            RewardsPoints = CurrentUser?.RewardsPoints ?? 0,
            CreatedAt = CurrentUser?.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task SaveProfileAsync()
    {
        ErrorMessage = string.Empty;

        // Validate inputs
        if (string.IsNullOrWhiteSpace(FullName))
        {
            ErrorMessage = "Full name is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Email is required";
            return;
        }

        IsLoading = true;
        try
        {
            if (CurrentUser != null)
            {
                CurrentUser.FullName = FullName;
                CurrentUser.Email = Email;
                CurrentUser.UpdatedAt = DateTime.UtcNow;

                await _databaseService.UpdateAsync(CurrentUser);

                SuccessMessage = "Profile updated successfully!";
                ShowSuccessMessage = true;

                IsEditMode = false;

                // Auto-hide success message after 3 seconds
                await Task.Delay(3000);
                ShowSuccessMessage = false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving profile: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void DiscardChanges()
    {
        if (CurrentUser != null)
        {
            FullName = CurrentUser.FullName;
            Email = CurrentUser.Email;
        }
        IsEditMode = false;
        ErrorMessage = string.Empty;
    }

    private async Task ChangePasswordAsync()
    {
        ErrorMessage = string.Empty;

        // Validate inputs
        if (string.IsNullOrWhiteSpace(OldPassword))
        {
            ErrorMessage = "Please enter your current password";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = "Please enter a new password";
            return;
        }

        if (NewPassword.Length < 8)
        {
            ErrorMessage = "New password must be at least 8 characters";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match";
            return;
        }

        IsChangingPassword = true;
        try
        {
            // Validate old password
            if (CurrentUser == null)
            {
                ErrorMessage = "User not found";
                return;
            }

            var isValidPassword = await _authService.ValidatePasswordAsync(CurrentUser.Email, OldPassword);
            if (!isValidPassword)
            {
                ErrorMessage = "Current password is incorrect";
                return;
            }

            // Update password (implementation in AuthService)
            var success = await _authService.ChangePasswordAsync(CurrentUser.Email, NewPassword);
            if (success)
            {
                SuccessMessage = "Password changed successfully!";
                ShowSuccessMessage = true;

                // Clear password fields
                OldPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;

                await Task.Delay(3000);
                ShowSuccessMessage = false;
            }
            else
            {
                ErrorMessage = "Failed to change password";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error changing password: {ex.Message}";
        }
        finally
        {
            IsChangingPassword = false;
        }
    }

    [Obsolete]
    private async Task LogoutAsync()
    {
        if (Application.Current?.MainPage == null)
            return;

        bool result = await Application.Current.MainPage.DisplayAlert(
            "Logout",
            "Are you sure you want to logout?",
            "Yes",
            "No");

        if (!result) return;

        IsLoading = true;
        try
        {
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error logging out: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AddAddressAsync()
    {
        // Navigate to add address modal
        await Shell.Current.GoToAsync("addressform");
    }

    private async Task EditAddressAsync(AddressItem? address)
    {
        if (address == null) return;

        // Navigate to edit address modal with address ID
        await Shell.Current.GoToAsync($"addressform?id={address.AddressId}");
    }

    [Obsolete]
    private async Task DeleteAddressAsync(AddressItem? address)
    {
        if (address == null) return;

        if (Application.Current?.MainPage == null)
            return;

        bool result = await Application.Current.MainPage.DisplayAlert(
            "Delete Address",
            "Are you sure you want to delete this address?",
            "Yes",
            "No");

        if (!result) return;

        IsLoading = true;
        try
        {
            // Mark address as deleted or physically delete
            await _databaseService.DeleteAsync<Address>(address.AddressId);
            Addresses.Remove(address);

            SuccessMessage = "Address deleted successfully";
            ShowSuccessMessage = true;

            await Task.Delay(2000);
            ShowSuccessMessage = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting address: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SetDefaultAddressAsync(AddressItem? address)
    {
        if (address == null || CurrentUser == null) return;

        IsLoading = true;
        try
        {
            // Clear default from all other addresses
            var allAddresses = await _databaseService.QueryAsync<Address>(
                "SELECT * FROM Addresses WHERE UserId = ?", CurrentUser.UserId);

            await _databaseService.ExecuteTransactionAsync(async () =>
            {
                foreach (var addr in allAddresses)
                {
                    addr.IsDefault = (addr.AddressId == address.AddressId);
                    addr.UpdatedAt = DateTime.UtcNow;
                    await _databaseService.UpdateAsync(addr);
                }
            });

            // Reload addresses
            await LoadAddressesAsync();

            SuccessMessage = "Default address updated";
            ShowSuccessMessage = true;

            await Task.Delay(2000);
            ShowSuccessMessage = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error updating default address: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SaveAddressAsync()
    {
        ErrorMessage = string.Empty;

        // Validate address
        if (CurrentAddress == null)
        {
            ErrorMessage = "Address information is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentAddress.AddressLine1))
        {
            ErrorMessage = "Street address is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentAddress.City))
        {
            ErrorMessage = "City is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentAddress.State))
        {
            ErrorMessage = "State is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentAddress.PostalCode))
        {
            ErrorMessage = "Postal code is required";
            return;
        }

        IsSaving = true;
        try
        {
            if (CurrentUser == null)
            {
                ErrorMessage = "User not found";
                return;
            }

            var address = new Address
            {
                AddressId = _editingAddressId ?? 0,
                UserId = CurrentUser.UserId,
                Label = CurrentAddress.Label,
                AddressLine1 = CurrentAddress.AddressLine1,
                AddressLine2 = CurrentAddress.AddressLine2,
                City = CurrentAddress.City,
                State = CurrentAddress.State,
                PostalCode = CurrentAddress.PostalCode,
                IsDefault = CurrentAddress.IsDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            bool success;
            if (_editingAddressId.HasValue && _editingAddressId.Value > 0)
            {
                // Update existing address
                success = await _authService.UpdateAddressAsync(address);
            }
            else
            {
                // Add new address
                success = await _authService.AddAddressAsync(address);
            }

            if (success)
            {
                SuccessMessage = _editingAddressId.HasValue ? "Address updated successfully!" : "Address added successfully!";
                ShowSuccessMessage = true;

                // Reload addresses
                await LoadAddressesAsync();

                // Reset form
                CurrentAddress = null;
                _editingAddressId = null;

                await Task.Delay(2000);
                ShowSuccessMessage = false;

                // Navigate back
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = "Failed to save address";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving address: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task CancelAddressFormAsync()
    {
        CurrentAddress = null;
        _editingAddressId = null;
        ErrorMessage = string.Empty;
        await Shell.Current.GoToAsync("..");
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

public class AddressItem
{
    public int AddressId { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string Label { get; set; } = "Home";
}
