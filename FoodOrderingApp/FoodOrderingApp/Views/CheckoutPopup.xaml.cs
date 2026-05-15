using FoodOrderingApp.Services;
using FoodOrderingApp.ViewModels;
using Microsoft.Maui.Controls;

namespace FoodOrderingApp.Views;

public partial class CheckoutPopup : ContentPage
{
    private readonly CheckoutViewModel _viewModel;

    public CheckoutPopup(CheckoutViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private void OnUPISelected(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            _viewModel.SelectedPaymentMethod = PaymentMethod.UPI;
            // Uncheck other checkboxes
            if (NetBankingFrame.Content is HorizontalStackLayout netBankingLayout &&
                netBankingLayout.Children[0] is CheckBox netBankingCheckbox)
            {
                netBankingCheckbox.IsChecked = false;
            }
            if (CreditCardFrame.Content is HorizontalStackLayout creditCardLayout &&
                creditCardLayout.Children[0] is CheckBox creditCardCheckbox)
            {
                creditCardCheckbox.IsChecked = false;
            }
            if (DebitCardFrame.Content is HorizontalStackLayout debitCardLayout &&
                debitCardLayout.Children[0] is CheckBox debitCardCheckbox)
            {
                debitCardCheckbox.IsChecked = false;
            }
        }
    }

    private void OnNetBankingSelected(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            _viewModel.SelectedPaymentMethod = PaymentMethod.NetBanking;
            // Uncheck other checkboxes
            if (UPIFrame.Content is HorizontalStackLayout upiLayout &&
                upiLayout.Children[0] is CheckBox upiCheckbox)
            {
                upiCheckbox.IsChecked = false;
            }
            if (CreditCardFrame.Content is HorizontalStackLayout creditCardLayout &&
                creditCardLayout.Children[0] is CheckBox creditCardCheckbox)
            {
                creditCardCheckbox.IsChecked = false;
            }
            if (DebitCardFrame.Content is HorizontalStackLayout debitCardLayout &&
                debitCardLayout.Children[0] is CheckBox debitCardCheckbox)
            {
                debitCardCheckbox.IsChecked = false;
            }
        }
    }

    private void OnCreditCardSelected(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            _viewModel.SelectedPaymentMethod = PaymentMethod.CreditCard;
            // Uncheck other checkboxes
            if (UPIFrame.Content is HorizontalStackLayout upiLayout &&
                upiLayout.Children[0] is CheckBox upiCheckbox)
            {
                upiCheckbox.IsChecked = false;
            }
            if (NetBankingFrame.Content is HorizontalStackLayout netBankingLayout &&
                netBankingLayout.Children[0] is CheckBox netBankingCheckbox)
            {
                netBankingCheckbox.IsChecked = false;
            }
            if (DebitCardFrame.Content is HorizontalStackLayout debitCardLayout &&
                debitCardLayout.Children[0] is CheckBox debitCardCheckbox)
            {
                debitCardCheckbox.IsChecked = false;
            }
        }
    }

    private void OnDebitCardSelected(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            _viewModel.SelectedPaymentMethod = PaymentMethod.DebitCard;
            // Uncheck other checkboxes
            if (UPIFrame.Content is HorizontalStackLayout upiLayout &&
                upiLayout.Children[0] is CheckBox upiCheckbox)
            {
                upiCheckbox.IsChecked = false;
            }
            if (NetBankingFrame.Content is HorizontalStackLayout netBankingLayout &&
                netBankingLayout.Children[0] is CheckBox netBankingCheckbox)
            {
                netBankingCheckbox.IsChecked = false;
            }
            if (CreditCardFrame.Content is HorizontalStackLayout creditCardLayout &&
                creditCardLayout.Children[0] is CheckBox creditCardCheckbox)
            {
                creditCardCheckbox.IsChecked = false;
            }
        }
    }
}
