using Microsoft.Maui.Controls;
namespace FoodOrderingApp.Views;

using FoodOrderingApp.ViewModels;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (BindingContext is ProfileViewModel viewModel)
        {
            await viewModel.LoadProfileAsync();
            await viewModel.LoadAddressesAsync();
        }
    }
}
