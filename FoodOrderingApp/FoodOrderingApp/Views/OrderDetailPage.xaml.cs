using Microsoft.Maui.Controls;
using FoodOrderingApp.ViewModels;

namespace FoodOrderingApp.Views;

public partial class OrderDetailPage : ContentPage
{
    private readonly OrderDetailViewModel _viewModel;

    public OrderDetailPage(OrderDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}
