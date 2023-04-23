using acControl.Properties;
using acControl.Scripts;
using acControl.Views.Pages;
using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace acControl.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        public ViewModels.MainWindowViewModel ViewModel
        {
            get;
        }

        INavigationService _navigationService;
        DispatcherTimer GC = new DispatcherTimer();
        public MainWindow(ViewModels.MainWindowViewModel viewModel, IPageService pageService, INavigationService navigationService)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();

            App.wmi.SubscribeToEvents(WatcherEventArrived);

            _ = Tablet.TabletDevices;
            SetPageService(pageService);
            if (Settings.Default.StartMini == true) { this.WindowState = WindowState.Minimized; }
            navigationService.SetNavigationControl(RootNavigation);
            _navigationService = navigationService;

            GC.Interval = TimeSpan.FromSeconds(2);
            GC.Tick += GC_Tick;
            GC.Start();

            Loaded += (sender, args) =>
            {
                Wpf.Ui.Appearance.Watcher.Watch(
                    this,                                  // Window class
                    Wpf.Ui.Appearance.BackgroundType.Mica, // Background type
                    true                                   // Whether to change accents automatically
                );
            };

            if (Global.isMinimalGUI == true)
            {
                this.MinWidth = 465;
                this.Width = 465;
                this.Height = 705;
                var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
                this.Left = desktopWorkingArea.Right - this.Width - 12;
                this.Top = desktopWorkingArea.Bottom - this.Height - 12;
                this.ResizeMode = ResizeMode.NoResize;
                tbMain.ShowMaximize = false;
            }
        }

        void WatcherEventArrived(object sender, EventArrivedEventArgs e)
        {

            var collection = (ManagementEventWatcher)sender;

            if (e.NewEvent is null) return;

            int EventID = int.Parse(e.NewEvent["EventID"].ToString());

            switch (EventID)
            {
                case 56:    // Rog button
                case 174:   // FN+F5
                    int profile = Settings.Default.ACMode;
                    profile++;
                    if (profile > 3) profile = 0;

                    if (profile == 0) ToastNotification.ShowToastNotification(false, "Switched to Silent", "Armoury Control has switched to the Silent power mode");
                    if (profile == 1) ToastNotification.ShowToastNotification(false, "Switched to Performance", "Armoury Control has switched to the Performance power mode");
                    if (profile == 2) ToastNotification.ShowToastNotification(false, "Switched to Turbo", "Armoury Control has switched to the Turbo power mode");
                    if (profile == 3) ToastNotification.ShowToastNotification(false, "Switched to Manual", "Armoury Control has switched to the Manual power mode");

                    Settings.Default.ACMode = profile;
                    Settings.Default.Save();
                    DashboardPage.updateProfile = true;
                    break;
                case 179:   // FN+F4
                    break;
            }
        }

        int i = 0;
        bool setup = false;
        async void GC_Tick(object sender, EventArgs e)
        {
            GarbageCollection.Garbage_Collect();

            if (i < 4) i++;

            if (i > 2 && setup == false)
            {
                if (Settings.Default.StartMini == true && this.WindowState == WindowState.Minimized) this.ShowInTaskbar = false;
                GC.Stop();
                GC.Interval = TimeSpan.FromSeconds(20);
                GC.Tick += GC_Tick;
                GC.Start();
                setup = true;
            }
        }

        #region INavigationWindow methods

        public Frame GetFrame()
            => RootFrame;

        public INavigation GetNavigation()
            => RootNavigation;

        public bool Navigate(Type pageType)
            => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService)
            => RootNavigation.PageService = pageService;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        private void HomeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized) this.WindowState = WindowState.Normal;
            _navigationService.Navigate(typeof(Views.Pages.DashboardPage));
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized) this.WindowState = WindowState.Normal;
            _navigationService.Navigate(typeof(Views.Pages.SettingsPage));
        }

        private void UiWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized) 
            { 
                this.ShowInTaskbar = false; 
                Global.isMinimised = true;
            }
            else
            {
                this.ShowInTaskbar = true;
                Global.isMinimised = false;

                if (Global.isMinimalGUI)
                {
                    var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
                    this.Left = desktopWorkingArea.Right - this.Width - 12;
                    this.Top = desktopWorkingArea.Bottom - this.Height - 12;
                }
            }
        }

        private void Silent_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ACMode = 0;
            Settings.Default.Save();
            DashboardPage.updateProfile = true;
        }
        private void Perf_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ACMode = 1;
            Settings.Default.Save();
            DashboardPage.updateProfile = true;
        }
        private void Turbo_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ACMode = 2;
            Settings.Default.Save();
            DashboardPage.updateProfile = true;
        }
        private void Man_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ACMode = 3;
            Settings.Default.Save();
            DashboardPage.updateProfile = true;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void CustomiseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized) this.WindowState = WindowState.Normal;
            _navigationService.Navigate(typeof(Views.Pages.CustomPresets));
        }

        private void NotifyIcon_LeftClick(Wpf.Ui.Controls.NotifyIcon sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                this.WindowState = WindowState.Minimized;
            } else
            {
                this.WindowState = WindowState.Normal;
                this.Activate();
            }
        }
    }
}