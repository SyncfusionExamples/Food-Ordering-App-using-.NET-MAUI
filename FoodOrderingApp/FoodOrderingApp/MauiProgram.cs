//using Microsoft.Extensions.Logging;

//namespace FoodOrderingApp
//{
//    public static class MauiProgram
//    {
//        public static MauiApp CreateMauiApp()
//        {
//            var builder = MauiApp.CreateBuilder();
//            builder
//                .UseMauiApp<App>()
//                .ConfigureFonts(fonts =>
//                {
//                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
//                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
//                });

//#if DEBUG
//    		builder.Logging.AddDebug();
//#endif

//            return builder.Build();
//        }
//    }
//}

using FoodOrderingApp.Database;
using FoodOrderingApp.Services;
using FoodOrderingApp.ViewModels;
using FoodOrderingApp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Syncfusion.Maui.Core;
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
        // Register Database Service
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

        // Register Authentication Service
        builder.Services.AddSingleton<IAuthService, AuthService>();

        // Register Cart Service
        builder.Services.AddSingleton<ICartService, CartService>();

        // Register Order Service
        builder.Services.AddSingleton<IOrderService, OrderService>();

        // Register Payment Service
        builder.Services.AddSingleton<IPaymentService, PaymentService>();

        // Register Map Service
        builder.Services.AddSingleton<IMapService, MapService>();

        // Register Validation Service
        builder.Services.AddSingleton<IValidationService, ValidationService>();

        // Register ViewModels
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<SignUpViewModel>();
        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<ItemDetailViewModel>();
        builder.Services.AddSingleton<CartViewModel>();
        builder.Services.AddSingleton<CheckoutViewModel>();
        builder.Services.AddSingleton<OrdersViewModel>();
        builder.Services.AddSingleton<OrderDetailViewModel>();
        builder.Services.AddSingleton<ProfileViewModel>();

        // Register Pages
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
