namespace FoodOrderingApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterModalRoutes();
    }

    private void RegisterModalRoutes()
    {
        Routing.RegisterRoute("itemdetail", typeof(Views.ItemDetailPopup));
        Routing.RegisterRoute("checkout", typeof(Views.CheckoutPopup));
        Routing.RegisterRoute("orderdetail", typeof(Views.OrderDetailPage));
        Routing.RegisterRoute("addressform", typeof(Views.AddressFormPopup));
    }
}

