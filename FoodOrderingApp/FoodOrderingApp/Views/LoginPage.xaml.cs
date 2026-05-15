using Microsoft.Maui.Controls;
using FoodOrderingApp.ViewModels;

namespace FoodOrderingApp.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
