using FoodOrderingApp.Services;
using FoodOrderingApp.ViewModels;

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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.ErrorMessage = string.Empty;
        _viewModel.SuccessMessage = string.Empty;
        _viewModel.ShowSuccessMessage = false;
        _viewModel.PaymentProcessing = false;
        _viewModel.IsLoading = false;
        _viewModel.SelectedPaymentMethod = PaymentMethod.UPI;
    }

    private void OnUPISelected(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            _viewModel.SelectedPaymentMethod = PaymentMethod.UPI;

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
