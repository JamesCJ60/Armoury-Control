﻿using acControl.Scripts;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace acControl.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _applicationTitle = String.Empty;

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationItems = new();

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationFooter = new();

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new();

        public MainWindowViewModel(INavigationService navigationService)
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            ApplicationTitle = "Armoury Control";

            NavigationItems = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "Home",
                    PageTag = "dashboard",
                    Icon = SymbolRegular.Home20,
                    PageType = typeof(Views.Pages.DashboardPage)
                },
                new NavigationItem()
                {
                    Content = "Custom",
                    PageTag = "presets",
                    Icon = SymbolRegular.Book20,
                    PageType = typeof(Views.Pages.CustomPresets)
                },
                new NavigationItem()
                {
                    Content = "Aura RGB",
                    PageTag = "aura",
                    Icon = SymbolRegular.Lightbulb20,
                    PageType = typeof(Views.Pages.AuraRGB)
                },
                new NavigationItem()
                {
                    Content = "AniMe",
                    PageTag = "anime",
                    Icon = SymbolRegular.GridDots20,
                    PageType = typeof(Views.Pages.AniMe)
                },
                new NavigationItem()
                {
                    Content = "Auto",
                    PageTag = "auto",
                    Icon = SymbolRegular.Transmission20,
                    PageType = typeof(Views.Pages.Auto)
                },
                //new NavigationItem()
                //{
                //    Content = "Data",
                //    PageTag = "data",
                //    Icon = SymbolRegular.DataHistogram24,
                //    PageType = typeof(Views.Pages.DataPage)
                //}
            };

            NavigationFooter = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "Settings",
                    PageTag = "settings",
                    Icon = SymbolRegular.Settings20,
                    PageType = typeof(Views.Pages.SettingsPage)
                }
            };

            TrayMenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem
                {
                    Header = "Home",
                    Tag = "tray_home"
                }
            };

            _isInitialized = true;
        }
    }
}
