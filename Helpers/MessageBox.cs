using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using DialogHostAvalonia;
using PSPSuite.Views.Components;

namespace PSPSuite.Helpers;

public static class MessageBox
{
    public static async Task Err(string title, string message)
    {
        var control = new MessageBoxControl
        {
            Title = title,
            Icon = IconState.ERROR,
            Message = message,
            Buttons = ButtonType.OK
        };

        control.ButtonClicked += (m) =>
        {
            DialogHost.Close(Constants.APP_NAME);
        };

        await DialogHost.Show(control, Constants.APP_NAME);
    }

    public static async Task Ok(string title, string message)
    {
        var control = new MessageBoxControl
        {
            Title = title,
            Icon = IconState.OK,
            Message = message,
            Buttons = ButtonType.OK
        };

        control.ButtonClicked += (m) =>
        {
            DialogHost.Close(Constants.APP_NAME);
        };

        await DialogHost.Show(control, Constants.APP_NAME);
    }

    public static async Task Info(string title, string message)
    {
        var control = new MessageBoxControl
        {
            Title = title,
            Icon = IconState.INFO,
            Message = message,
            Buttons = ButtonType.OK
        };

        control.ButtonClicked += (m) =>
        {
            DialogHost.Close(Constants.APP_NAME);
        };

        await DialogHost.Show(control, Constants.APP_NAME);
    }

    public static async Task Confirm(string title, string message, Action? action = null)
    {
        var control = new MessageBoxControl
        {
            Title = title,
            Icon = IconState.CONFIRM,
            Message = message,
            Buttons = ButtonType.OK_CANCEL
        };

        control.ButtonClicked += (m) =>
        {
            if (m == MessageBoxResult.OK)
            {
                action?.Invoke();
            }

            DialogHost.Close(Constants.APP_NAME);
        };

        await DialogHost.Show(control, Constants.APP_NAME);
    }
}