using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using NickvisionMoney.WinUI.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.UI;

namespace NickvisionMoney.WinUI.Views;

/// <summary>
/// A dialog for managing a Transaction
/// </summary>
public sealed partial class TransactionDialog : ContentDialog
{
    private readonly TransactionDialogController _controller;
    private readonly Action<object> _initializeWithWindow;
    private string? _receiptPath;

    /// <summary>
    /// Constructs a TransactionDialog
    /// </summary>
    /// <param name="controller">The TransactionDialogController</param>
    /// <param name="initializeWithWindow">The Action<object> callback for InitializeWithWindow</param>
    public TransactionDialog(TransactionDialogController controller, Action<object> initializeWithWindow)
    {
        InitializeComponent();
        _controller = controller;
        _initializeWithWindow = initializeWithWindow;
        _receiptPath = null;
        //Localize Strings
        Title = $"{_controller.Localizer["Transaction"]} - {_controller.Transaction.Id}";
        CloseButtonText = _controller.Localizer["Cancel"];
        PrimaryButtonText = _controller.Localizer["OK"];
        TxtDescription.Header = _controller.Localizer["Description", "Field"];
        TxtDescription.PlaceholderText = _controller.Localizer["Description", "Placeholder"];
        TxtAmount.Header = $"{_controller.Localizer["Amount", "Field"]} -  {_controller.CultureForNumberString.NumberFormat.CurrencySymbol} {(string.IsNullOrEmpty(_controller.CultureForNumberString.NumberFormat.NaNSymbol) ? "" : $"({_controller.CultureForNumberString.NumberFormat.NaNSymbol})")}";
        TxtAmount.PlaceholderText = _controller.Localizer["Amount", "Placeholder"];
        CmbType.Header = _controller.Localizer["TransactionType", "Field"];
        CmbType.Items.Add(_controller.Localizer["Income"]);
        CmbType.Items.Add(_controller.Localizer["Expense"]);
        CalendarDate.Header = _controller.Localizer["Date", "Field"];
        CmbGroup.Header = _controller.Localizer["Group", "Field"];
        CmbGroup.Items.Add(_controller.Localizer["Ungrouped"]);
        CmbRepeatInterval.Header = _controller.Localizer["TransactionRepeatInterval", "Field"];
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Never"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Daily"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Weekly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Biweekly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Monthly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Quarterly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Yearly"]);
        CmbRepeatInterval.Items.Add(_controller.Localizer["RepeatInterval", "Biyearly"]);
        CalendarRepeatEndDate.Header = _controller.Localizer["TransactionRepeatEndDate", "Field"];
        ToolTipService.SetToolTip(BtnRepeatEndDateClear, controller.Localizer["TransactionRepeatEndDate", "Clear"]);
        LblColor.Text = _controller.Localizer["Color", "Field"];
        LblReceipt.Text = _controller.Localizer["Receipt", "Field"];
        LblBtnReceiptView.Text = _controller.Localizer["View"];
        LblBtnReceiptDelete.Text = _controller.Localizer["Delete"];
        LblBtnReceiptUpload.Text = _controller.Localizer["Upload"];
        TxtErrors.Text = _controller.Localizer["FixErrors", "WinUI"];
        //Load Transaction
        TxtDescription.Text = _controller.Transaction.Description;
        TxtAmount.Text = _controller.Transaction.Amount.ToString("C", _controller.CultureForNumberString);
        CmbType.SelectedIndex = (int)_controller.Transaction.Type;
        CalendarDate.Date = new DateTimeOffset(new DateTime(_controller.Transaction.Date.Year, _controller.Transaction.Date.Month, _controller.Transaction.Date.Day));
        var groups = _controller.Groups.Values.ToList();
        groups.Sort();
        foreach (var group in groups)
        {
            CmbGroup.Items.Add(group);
        }
        if (_controller.Transaction.GroupId == -1)
        {
            CmbGroup.SelectedIndex = 0;
        }
        else
        {
            CmbGroup.SelectedItem = _controller.Groups[(uint)_controller.Transaction.GroupId];
        }
        CmbRepeatInterval.SelectedIndex = (int)_controller.RepeatIntervalIndex;
        CalendarRepeatEndDate.IsEnabled = _controller.Transaction.RepeatInterval != TransactionRepeatInterval.Never;
        BtnRepeatEndDateClear.IsEnabled = _controller.Transaction.RepeatInterval != TransactionRepeatInterval.Never;
        if (_controller.Transaction.RepeatEndDate != null)
        {
            CalendarRepeatEndDate.Date = new DateTimeOffset(new DateTime(_controller.Transaction.RepeatEndDate.Value.Year, _controller.Transaction.RepeatEndDate.Value.Month, _controller.Transaction.RepeatEndDate.Value.Day));
        }
        BtnColor.SelectedColor = (Color)ColorHelpers.FromRGBA(_controller.Transaction.RGBA)!;
        BtnReceiptView.IsEnabled = _controller.Transaction.Receipt != null;
        BtnReceiptDelete.IsEnabled = _controller.Transaction.Receipt != null;
    }

    /// <summary>
    /// Shows the TransactionDialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public new async Task<bool> ShowAsync()
    {
        var result = await base.ShowAsync();
        if (result == ContentDialogResult.None)
        {
            _controller.Accepted = false;
            return false;
        }
        else if (result == ContentDialogResult.Primary)
        {
            var checkStatus = _controller.UpdateTransaction(DateOnly.FromDateTime(CalendarDate.Date!.Value.Date), TxtDescription.Text, (TransactionType)CmbType.SelectedIndex, CmbRepeatInterval.SelectedIndex, (string)CmbGroup.SelectedItem, ColorHelpers.ToRGBA(BtnColor.SelectedColor), TxtAmount.Text, _receiptPath, CalendarRepeatEndDate.Date == null ? null : DateOnly.FromDateTime(CalendarRepeatEndDate.Date!.Value.Date));
            if(checkStatus != TransactionCheckStatus.Valid)
            {
                TxtDescription.Header = _controller.Localizer["Description", "Field"];
                TxtAmount.Header = $"{_controller.Localizer["Amount", "Field"]} -  {_controller.CultureForNumberString.NumberFormat.CurrencySymbol} {(string.IsNullOrEmpty(_controller.CultureForNumberString.NumberFormat.NaNSymbol) ? "" : $"({_controller.CultureForNumberString.NumberFormat.NaNSymbol})")}";
                CalendarRepeatEndDate.Header = _controller.Localizer["TransactionRepeatEndDate", "Field"];
                if (checkStatus == TransactionCheckStatus.EmptyDescription)
                {
                    TxtDescription.Header = _controller.Localizer["Description", "Empty"];
                }
                else if(checkStatus == TransactionCheckStatus.InvalidAmount)
                {
                    TxtAmount.Header = _controller.Localizer["Amount", "Invalid"];
                }
                else if(checkStatus == TransactionCheckStatus.InvalidRepeatEndDate)
                {
                    CalendarRepeatEndDate.Header = _controller.Localizer["TransactionRepeatEndDate", "Invalid"];
                }
                TxtErrors.Visibility = Visibility.Visible;
                return await ShowAsync();
            }
            else
            {
                _controller.Accepted = true;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Occurs when the repeat interval combobox is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void CmbRepeatInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var isRepeatIntervalNever = (string)CmbRepeatInterval.SelectedItem == _controller.Localizer["RepeatInterval", "Never"];
        CalendarRepeatEndDate.IsEnabled = !isRepeatIntervalNever;
        BtnRepeatEndDateClear.IsEnabled = !isRepeatIntervalNever;
    }

    /// <summary>
    /// Occurs when the clear repeat end date button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ClearRepeatEndDate(object sender, RoutedEventArgs e) => CalendarRepeatEndDate.Date = null;

    /// <summary>
    /// Occurs when the view receipt button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void ViewReceipt(object sender, RoutedEventArgs e) => await _controller.OpenReceiptImageAsync(_receiptPath);

    /// <summary>
    /// Occurs when the delete receipt button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void DeleteReceipt(object sender, RoutedEventArgs e)
    {
        _receiptPath = "";
        BtnReceiptView.IsEnabled = false;
        BtnReceiptDelete.IsEnabled = false;
    }

    /// <summary>
    /// Occurs when the upload receipt button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void UploadReceipt(object sender, RoutedEventArgs e)
    {
        var fileOpenPicker = new FileOpenPicker();
        _initializeWithWindow(fileOpenPicker);
        fileOpenPicker.FileTypeFilter.Add(".jpg");
        fileOpenPicker.FileTypeFilter.Add(".jpeg");
        fileOpenPicker.FileTypeFilter.Add(".png");
        fileOpenPicker.FileTypeFilter.Add(".pdf");
        fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        var file = await fileOpenPicker.PickSingleFileAsync();
        if (file != null)
        {
            _receiptPath = file.Path;
            BtnReceiptView.IsEnabled = true;
        }
    }
}
