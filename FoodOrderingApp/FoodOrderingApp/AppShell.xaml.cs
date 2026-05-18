namespace FoodOrderingApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register modal/popup routes only
        // Tab and shell content routes are handled by XAML declarations
        RegisterModalRoutes();
    }

    private void RegisterModalRoutes()
    {
        // Modal/Popup Routes - only register routes not in XAML shell content
        // Shell content routes (login, signup, home, cart, orders, profile) are already defined in AppShell.xaml
        Routing.RegisterRoute("itemdetail", typeof(Views.ItemDetailPopup));
        Routing.RegisterRoute("checkout", typeof(Views.CheckoutPopup));
        Routing.RegisterRoute("orderdetail", typeof(Views.OrderDetailPage));
        Routing.RegisterRoute("addressform", typeof(Views.AddressFormPopup));
    }
}

