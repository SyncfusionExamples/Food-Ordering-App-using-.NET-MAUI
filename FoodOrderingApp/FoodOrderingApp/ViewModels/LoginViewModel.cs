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
        NavigateToSignUpCommand = new RelayCommand(NavigateToSignUp);
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
                // Show main tabs
                if (App.Current?.MainPage is AppShell shell)
                {
                    shell.ShowMainTabs();
                }
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

    private void NavigateToSignUp()
    {
        Shell.Current?.GoToAsync("signup");
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

    public bool CanExecute(object? parameter) => !_isExecuting;

    public async void Execute(object? parameter)
    {
        _isExecuting = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        try
        {
            await _execute();
        }
        finally
        {
            _isExecuting = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
