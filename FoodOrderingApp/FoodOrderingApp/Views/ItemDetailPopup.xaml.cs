using Microsoft.Maui.Controls;
using FoodOrderingApp.ViewModels;

namespace FoodOrderingApp.Views;

public partial class ItemDetailPopup : ContentPage
{
    public ItemDetailPopup(ItemDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
