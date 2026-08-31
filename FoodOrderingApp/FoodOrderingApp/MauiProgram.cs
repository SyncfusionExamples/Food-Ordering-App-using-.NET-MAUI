using FoodOrderingApp.Database;
using FoodOrderingApp.Services;
using FoodOrderingApp.ViewModels;
using FoodOrderingApp.Views;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace FoodOrderingApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
            })
            .ConfigureSyncfusionCore()
            .ConfigureServices();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<ICartService, CartService>();
        builder.Services.AddSingleton<IOrderService, OrderService>();
        builder.Services.AddSingleton<IPaymentService, PaymentService>();
        builder.Services.AddSingleton<IMapService, MapService>();
        builder.Services.AddSingleton<IValidationService, ValidationService>();

        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<SignUpViewModel>();
        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<ItemDetailViewModel>();
        builder.Services.AddSingleton<CartViewModel>();
        builder.Services.AddSingleton<CheckoutViewModel>();
        builder.Services.AddSingleton<OrdersViewModel>();
        builder.Services.AddSingleton<OrderDetailViewModel>();
        builder.Services.AddSingleton<ProfileViewModel>();

        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<SignUpPage>();
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<ItemDetailPopup>();
        builder.Services.AddSingleton<CartPage>();
        builder.Services.AddSingleton<CheckoutPopup>();
        builder.Services.AddSingleton<OrdersPage>();
        builder.Services.AddSingleton<OrderDetailPage>();
        builder.Services.AddSingleton<ProfilePage>();
        builder.Services.AddSingleton<AddressFormPopup>();

        return builder;
    }
}
