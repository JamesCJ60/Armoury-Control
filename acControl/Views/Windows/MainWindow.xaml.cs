using acControl.Scripts;
using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
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
        public MainWindow(ViewModels.MainWindowViewModel viewModel, IPageService pageService, INavigationService navigationService)
        {
            ViewModel = viewModel;
            DataContext = this;

            

            InitializeComponent();
            SetPageService(pageService);

            navigationService.SetNavigationControl(RootNavigation);
            _navigationService = navigationService;
            DispatcherTimer GC = new DispatcherTimer();
            GC.Interval = TimeSpan.FromSeconds(22);
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

            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
                hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
        }

        async void GC_Tick(object sender, EventArgs e)
        {
            GarbageCollection.Garbage_Collect();
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
            if (this.WindowState == WindowState.Minimized) this.ShowInTaskbar = false;
            else this.ShowInTaskbar = true;
        }
    }
}