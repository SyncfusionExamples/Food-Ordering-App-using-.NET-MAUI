using FoodOrderingApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Linq;

namespace FoodOrderingApp;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly IAuthService _authService;

    public App(IAuthService authService)
    {
        InitializeComponent();

        _authService = authService;
    }

    // Helper to access the currently active Window (replaces the nonexistent 'MainWindow')
    private Microsoft.Maui.Controls.Window? MainWindow => Microsoft.Maui.Controls.Application.Current?.Windows?.FirstOrDefault();

    protected override Microsoft.Maui.Controls.Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell();
        var window = new Microsoft.Maui.Controls.Window(shell);
        
        // Initialize app in background but don't block window creation
        _ = InitializeAppAsync(shell);
        
        return window;
    }

    private async Task InitializeAppAsync(AppShell shell)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("App: Starting initialization...");
            
            // Initialize database first with generous timeout (30 seconds for first run)
            using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                try
                {
                    var dbService = IPlatformApplication.Current?.Services.GetService<IDatabaseService>();
                    if (dbService != null)
                    {
                        System.Diagnostics.Debug.WriteLine("App: Initializing database...");
                        await dbService.InitializeAsync();
                        System.Diagnostics.Debug.WriteLine("App: Database initialized successfully");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("App: DatabaseService not found in DI container");
                    }
                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("App: Database initialization timed out after 30 seconds");
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"App: Database initialization error: {dbEx.Message}\n{dbEx.StackTrace}");
                }
            }

            // Load session cache from secure storage
            System.Diagnostics.Debug.WriteLine("App: Loading session cache...");
            await _authService.IsSessionValidAsync();  // This loads the cache
            
            // Check if user is already logged in
            var isLoggedIn = _authService.IsSessionValid();
            System.Diagnostics.Debug.WriteLine($"App: User logged in: {isLoggedIn}");

            // Navigate to appropriate page based on session
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (isLoggedIn)
                    {
                        System.Diagnostics.Debug.WriteLine("App: User is logged in, navigating to home...");
                        await shell.GoToAsync("//home", animate: false);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("App: User not logged in, navigating to login...");
                        await shell.GoToAsync("//login", animate: false);
                    }
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"App: Navigation error: {navEx.Message}");
                    // Fallback to login if navigation fails
                    try
                    {
                        await shell.GoToAsync("//login", animate: false);
                    }
                    catch (Exception fallbackEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"App: Fallback navigation error: {fallbackEx.Message}");
                    }
                }
            });
            
            System.Diagnostics.Debug.WriteLine("App: Initialization complete");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"App Initialization error: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
