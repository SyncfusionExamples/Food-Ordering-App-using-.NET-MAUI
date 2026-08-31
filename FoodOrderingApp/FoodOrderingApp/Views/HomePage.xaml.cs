using Microsoft.Maui.Controls;
using FoodOrderingApp.ViewModels;
using FoodOrderingApp.Models;

namespace FoodOrderingApp.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is HomeViewModel vm && e.CurrentSelection.FirstOrDefault() is Item item)
        {
            vm.ItemSelectedCommand.Execute(item);
        }
    }

}
