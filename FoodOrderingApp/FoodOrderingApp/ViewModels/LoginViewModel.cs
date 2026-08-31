using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FoodOrderingApp.Services;
using Microsoft.Maui.Controls;

namespace FoodOrderingApp.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly IAuthService _authService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;
    private bool _isPasswordVisible = false;

    public event PropertyChangedEventHandler? PropertyChanged;

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

    public ICommand LoginCommand { get; }
    public ICommand NavigateToSignUpCommand { get; }
    public ICommand TogglePasswordVisibilityCommand { get; }

    [Obsolete]
    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;

        LoginCommand = new AsyncRelayCommand(LoginAsync);
        NavigateToSignUpCommand = new AsyncRelayCommand(NavigateToSignUpAsync);
        TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
    }

    [Obsolete]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email and password are required";
            return;
        }

        IsLoading = true;

        try
        {
            var result = await _authService.LoginAsync(Email, Password);

            if (result.IsSuccessful)
            {
                // Navigate to Home page
                await Shell.Current.GoToAsync("//home");
                System.Diagnostics.Debug.WriteLine("LoginViewModel: Navigation to home complete");
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Login failed";
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

    private async Task NavigateToSignUpAsync()
    {
        try
        {
            ResetFields();
            await Shell.Current.GoToAsync("//signup");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Navigation error: {ex.Message}";
        }
    }

    public void ResetFields()
    {
        Email = string.Empty;
        Password = string.Empty;
        ErrorMessage = string.Empty;
        IsPasswordVisible = false;
    }

    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
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

// Helper command classes
public class RelayCommand : ICommand
{
    private readonly Action _execute;

    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action execute)
    {
        _execute = execute;
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
        _execute();
    }
}

public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private bool _isExecuting = false;

    public event EventHandler? CanExecuteChanged;

    public AsyncRelayCommand(Func<Task> execute)
    {
        _execute = execute;
    }

    public bool CanExecute(object? parameter)
    {
        System.Diagnostics.Debug.WriteLine($"AsyncRelayCommand.CanExecute called - IsExecuting: {_isExecuting}");
        return !_isExecuting;
    }

    public void Execute(object? parameter)
    {
        System.Diagnostics.Debug.WriteLine("AsyncRelayCommand.Execute called");
        
        // Fire and forget with proper exception handling
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (_isExecuting)
            {
                System.Diagnostics.Debug.WriteLine("AsyncRelayCommand: Already executing, ignoring");
                return;
            }

            _isExecuting = true;
            System.Diagnostics.Debug.WriteLine("AsyncRelayCommand: Setting IsExecuting = true");
            
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

            try
            {
                System.Diagnostics.Debug.WriteLine("AsyncRelayCommand: Starting async operation");
                await _execute();
                System.Diagnostics.Debug.WriteLine("AsyncRelayCommand: Async operation completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AsyncRelayCommand: Exception - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"AsyncRelayCommand: StackTrace - {ex.StackTrace}");
            }
            finally
            {
                _isExecuting = false;
                System.Diagnostics.Debug.WriteLine("AsyncRelayCommand: Setting IsExecuting = false");
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        });
    }
}
