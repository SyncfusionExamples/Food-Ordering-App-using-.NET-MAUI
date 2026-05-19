using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FoodOrderingApp.Services;
using Microsoft.Maui.Controls;

namespace FoodOrderingApp.ViewModels;

public partial class SignUpViewModel : INotifyPropertyChanged
{
    private readonly IAuthService _authService;
    private string _fullName = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;
    private bool _isPasswordVisible = false;
    private bool _isConfirmPasswordVisible = false;
    private const int MinPasswordLength = 8;

    public event PropertyChangedEventHandler? PropertyChanged;

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

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
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

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsPasswordVisible
    {
        get => _isPasswordVisible;
        set => SetProperty(ref _isPasswordVisible, value);
    }

    public bool IsConfirmPasswordVisible
    {
        get => _isConfirmPasswordVisible;
        set => SetProperty(ref _isConfirmPasswordVisible, value);
    }

    public ICommand SignUpCommand { get; }
    public ICommand NavigateToLoginCommand { get; }
    public ICommand TogglePasswordVisibilityCommand { get; }
    public ICommand ToggleConfirmPasswordVisibilityCommand { get; }

    public SignUpViewModel(IAuthService authService)
    {
        _authService = authService;

        SignUpCommand = new AsyncRelayCommand(SignUpAsync);
        NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
        TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
        ToggleConfirmPasswordVisibilityCommand = new RelayCommand(ToggleConfirmPasswordVisibility);
    }

    private async Task SignUpAsync()
    {
        ErrorMessage = string.Empty;

        // Validation
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "All fields are required";
            return;
        }

        if (Password.Length < MinPasswordLength)
        {
            ErrorMessage = $"Password must be at least {MinPasswordLength} characters long";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match";
            return;
        }

        IsLoading = true;

        try
        {
            var result = await _authService.SignUpAsync(FullName, Email, Password);

            if (result.IsSuccessful)
            {
                // Navigate back to login page
                await Shell.Current.GoToAsync("//login");
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Sign up failed";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void NavigateToLogin()
    {
        ResetFields();
        Shell.Current?.GoToAsync("//login");
    }

    public void ResetFields()
    {
        FullName = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        ErrorMessage = string.Empty;
        IsPasswordVisible = false;
        IsConfirmPasswordVisible = false;
    }

    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    private void ToggleConfirmPasswordVisibility()
    {
        IsConfirmPasswordVisible = !IsConfirmPasswordVisible;
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
