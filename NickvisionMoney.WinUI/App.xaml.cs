﻿using Microsoft.UI.Xaml;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using NickvisionMoney.WinUI.Views;
using System;

namespace NickvisionMoney.WinUI;

public partial class App : Application
{
    private Window? _mainWindow;
    private readonly MainWindowController _mainWindowController;

    public App()
    {
        InitializeComponent();
        _mainWindowController = new MainWindowController();
        //AppInfo
        _mainWindowController.AppInfo.ID = "org.nickvision.money";
        _mainWindowController.AppInfo.Name = "Nickvision Denaro";
        _mainWindowController.AppInfo.ShortName = "Denaro";
        _mainWindowController.AppInfo.Description = $"{_mainWindowController.Localizer["Description"]}.";
        _mainWindowController.AppInfo.Version = "2023.1.0-rc1";
        _mainWindowController.AppInfo.Changelog = "- Money has been completely rewritten in C# and now has a new name: Denaro!\nWith the C# rewrite, there is now a new version of Denaro available on Windows!\n- Added an Account Settings dialog to allow users to customize their accounts better\n- Added an \"Ungrouped\" row to the groups section to allow filtering transactions that don't belong to a group\n- Added the ability to attach a jpg/png/pdf of a receipt to a transaction\n- Reworked the repeat transaction system and added support for a biweekly interval\n- Added the ability to export an account as a PDF\n- Added the ability to sort transactions by id or date\n- Added the ability to hide the groups section\n- Made a group's description an optional field";
        _mainWindowController.AppInfo.GitHubRepo = new Uri("https://github.com/nlogozzo/NickvisionMoney");
        _mainWindowController.AppInfo.IssueTracker = new Uri("https://github.com/nlogozzo/NickvisionMoney/issues/new");
        _mainWindowController.AppInfo.SupportUrl = new Uri("https://github.com/nlogozzo/NickvisionMoney/discussions");
        //Theme
        if (_mainWindowController.Theme == Theme.Light)
        {
            RequestedTheme = ApplicationTheme.Light;
        }
        else if (_mainWindowController.Theme == Theme.Dark)
        {
            RequestedTheme = ApplicationTheme.Dark;
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow(_mainWindowController);
        _mainWindow.Activate();
    }
}