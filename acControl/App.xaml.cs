﻿using acControl.Models;
using acControl.Properties;
using acControl.Scripts;
using acControl.Services;
using acControl.Views.Pages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace acControl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App
    {
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)); })
            .ConfigureServices((context, services) =>
            {
                // App Host
                services.AddHostedService<ApplicationHostService>();

                // Page resolver service
                services.AddSingleton<IPageService, PageService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Main window with navigation
                services.AddScoped<INavigationWindow, Views.Windows.MainWindow>();
                services.AddScoped<ViewModels.MainWindowViewModel>();

                // Views and ViewModels
                services.AddScoped<Views.Pages.DashboardPage>();
                services.AddScoped<Views.Pages.CustomPresets>();
                services.AddScoped<Views.Pages.AuraRGB>();
                services.AddScoped<ViewModels.DashboardViewModel>();
                services.AddScoped<Views.Pages.DataPage>();
                services.AddScoped<ViewModels.DataViewModel>();
                services.AddScoped<Views.Pages.SettingsPage>();
                services.AddScoped<ViewModels.SettingsViewModel>();

                // Configuration
                services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
            }).Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        public static string location;

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        /// 
        public static ASUSWmi wmi = new ASUSWmi();
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                if (!App.IsAdministrator())
                {
                    // Restart and run as admin
                    var exeName = Process.GetCurrentProcess().MainModule.FileName;
                    ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                    startInfo.Verb = "runas";
                    startInfo.UseShellExecute = true;
                    startInfo.Arguments = "restart";
                    Process.Start(startInfo);
                    Environment.Exit(0);
                }

                _ = Tablet.TabletDevices;

                location = AppDomain.CurrentDomain.BaseDirectory;

                GetSystemInfo.start();
                GetSystemInfo.getDisplayData();
                GetSystemInfo.getBattery();
                GetSystemInfo.CurrentDisplayRrefresh();
                wmi.SubscribeToEvents(WatcherEventArrived);

                await _host.StartAsync();
            } catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());  
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }

        static async void WatcherEventArrived(object sender, EventArrivedEventArgs e)
        {
            await Task.Run(() =>
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
                        if (profile > 2) profile = 0;

                        Settings.Default.ACMode = profile;
                        Settings.Default.Save();
                        DashboardPage.updateProfile = true;
                        break;
                    case 179:   // FN+F4
                        break;
                }
            });
        }
    }
}