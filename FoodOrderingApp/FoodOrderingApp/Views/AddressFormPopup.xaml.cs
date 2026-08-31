namespace FoodOrderingApp.Views;

using FoodOrderingApp.ViewModels;
using Microsoft.Maui.Controls;

public partial class AddressFormPopup : ContentPage
{
    public AddressFormPopup()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // Prevent closing on back button
        if (BindingContext is ProfileViewModel viewModel)
        {
            // ViewModel handles address initialization
        }
    }
}
