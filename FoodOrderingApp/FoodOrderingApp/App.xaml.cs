using FoodOrderingApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace FoodOrderingApp;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly IAuthService _authService;

    public App(IAuthService authService)
    {
        InitializeComponent();

        _authService = authService;

        MainPage = new AppShell();
    }

    protected override async void OnStart()
    {
        base.OnStart();
        await InitializeAppAsync();
    }

    private async Task InitializeAppAsync() 
    {
        // Initialize database on app start
        var dbService = IPlatformApplication.Current?.Services.GetService<IDatabaseService>();
        if (dbService != null)
        {
            await dbService.InitializeAsync();
        }

        // Check if user has a valid session
        var isLoggedIn = _authService.IsSessionValid();

        await Shell.Current.Navigation.PopToRootAsync();
        if (isLoggedIn && MainPage is AppShell shell)
        {
            //shell.ShowMainTabs();
            await Shell.Current.GoToAsync("home", animate: false);
        }
        else if (MainPage is AppShell shell2)
        {
            //shell2.ShowAuthPages();
            await Shell.Current.GoToAsync("login", animate: false);
        }
    }
}
