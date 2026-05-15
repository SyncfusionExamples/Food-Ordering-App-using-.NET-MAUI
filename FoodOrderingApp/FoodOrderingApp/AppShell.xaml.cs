namespace FoodOrderingApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("login", typeof(Views.LoginPage));
        Routing.RegisterRoute("signup", typeof(Views.SignUpPage));

        Routing.RegisterRoute("home", typeof(Views.HomePage));
        Routing.RegisterRoute("cart", typeof(Views.CartPage));
        Routing.RegisterRoute("orders", typeof(Views.OrdersPage));
        Routing.RegisterRoute("profile", typeof(Views.ProfilePage));

        Routing.RegisterRoute("itemdetail", typeof(Views.ItemDetailPopup));
        Routing.RegisterRoute("checkout", typeof(Views.CheckoutPopup));
        Routing.RegisterRoute("orderdetail", typeof(Views.OrderDetailPage));

        Routing.RegisterRoute("signup", typeof(Views.SignUpPage));
    }

    public void ShowMainTabs()
    {
        LoginRoute.IsVisible = false;
        SignUpRoute.IsVisible = false;
        MainTabBar.IsVisible = true;
    }

    public void ShowAuthPages()
    {
        LoginRoute.IsVisible = true;
        SignUpRoute.IsVisible = true;
        MainTabBar.IsVisible = false;
    }

}

