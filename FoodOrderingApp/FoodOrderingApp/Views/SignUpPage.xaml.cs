using Microsoft.Maui.Controls;
using FoodOrderingApp.ViewModels;

namespace FoodOrderingApp.Views;

public partial class SignUpPage : ContentPage
{
    public SignUpPage(SignUpViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
