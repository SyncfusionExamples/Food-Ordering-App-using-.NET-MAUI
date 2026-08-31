using Microsoft.Maui.Controls;
using FoodOrderingApp.ViewModels;
using System;

namespace FoodOrderingApp.Views;

public partial class OrdersPage : ContentPage
{
    private readonly OrdersViewModel _viewModel;

    public OrdersPage(OrdersViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    private async void OnStartOrderingClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//home");
    }
}
