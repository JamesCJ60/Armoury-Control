using acControl.Models;
using acControl.Properties;
using acControl.Scripts;
using acControl.Services;
using acControl.Views.Pages;
using acControl.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using System.ServiceProcess;
using static acControl.Scripts.SystemDeviceControl;

namespace acControl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public static ASUSWmi wmi = new ASUSWmi();
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
                services.AddSingleton(wmi);
                services.AddSingleton<XgMobileConnectionService>();

                // Main window with navigation
                services.AddScoped<INavigationWindow, Views.Windows.MainWindow>();
                services.AddScoped<ViewModels.MainWindowViewModel>();

                // Views and ViewModels
                services.AddScoped<Views.Pages.DashboardPage>();
                services.AddScoped<Views.Pages.CustomPresets>();
                services.AddScoped<Views.Pages.AuraRGB>();
                services.AddScoped<Views.Pages.AniMe>();
                services.AddScoped<Views.Pages.Auto>();
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

                Global.isMinimalGUI = Settings.Default.MinimalGUI;

                _ = Tablet.TabletDevices;

                location = AppDomain.CurrentDomain.BaseDirectory;
                xgMobileConnectionService = GetService<XgMobileConnectionService>();
                GetSystemInfo.getDisplayData();
                GetSystemInfo.getBattery();
                GetSystemInfo.CurrentDisplayRrefresh();
                SetUpXgMobileDetection();

                SystemEvents.PowerModeChanged += OnPowerModeChanged;

                await _host.StartAsync();
            }
            catch (Exception ex)
            {
                Environment.Exit(0);
                App.wmi.Close();
            }
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if(Settings.Default.ModernStandby == true)
            {
                try
                {
                    switch (e.Mode)
                    {
                        case PowerModes.Suspend:
                            // Disable Windows Update and Microsoft Store Service
                            DisableServices("UpdateOrchestrator", "wuauserv", "InstallService");
                            // Prevent the services from starting automatically
                            PreventServiceAutoStart("UpdateOrchestrator", "wuauserv", "InstallService");
                            break;
                        case PowerModes.Resume:
                            // Enable Windows Update and Microsoft Store Service
                            EnableServices("UpdateOrchestrator", "wuauserv", "InstallService");
                            // Allow the services to start automatically
                            AllowServiceAutoStart("UpdateOrchestrator", "wuauserv", "InstallService");
                            break;
                    }
                }
                catch { }
            }

        }

        private void DisableServices(params string[] serviceNames)
        {
            foreach (string serviceName in serviceNames)
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                }
            }
        }

        private void EnableServices(params string[] serviceNames)
        {
            foreach (string serviceName in serviceNames)
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                }
            }
        }

        private void PreventServiceAutoStart(params string[] serviceNames)
        {
            foreach (string serviceName in serviceNames)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "sc.exe";
                startInfo.Arguments = $"config {serviceName} start= disabled";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                Process process = Process.Start(startInfo);
                process.WaitForExit();
            }
        }

        private void AllowServiceAutoStart(params string[] serviceNames)
        {
            foreach (string serviceName in serviceNames)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "sc.exe";
                startInfo.Arguments = $"config {serviceName} start= auto";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                Process process = Process.Start(startInfo);
                process.WaitForExit();
            }
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            App.wmi.Close();
            await _host.StopAsync();

            _host.Dispose();
            ToastNotification.HideXgMobileActivateToasts();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }

        

        public static Guid DLAHI_GUID = new Guid("{5c4c3332-344d-483c-8739-259e934c9cc8}");
        public static string DLAHI_Instance = @"SWD\DRIVERENUM\OEM_DAL_COMPONENT&4&293F28F0&0";

        public static Guid DTTDE_GUID = new Guid("{5c4c3332-344d-483c-8739-259e934c9cc8}");
        public static string DTTDE_Instance = @"SWD\DRIVERENUM\{BC7814A1-A80E-44B3-87C6-652EAC676387}#DTTEXTCOMPONENT&4&DE2304&3";

        private async void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (MotherboardInfo.Product.Contains("Flow Z13") && Settings.Default.StartOnBoot == true)
                    {
                        DeviceHelper.SetDeviceEnabled(DLAHI_GUID, DLAHI_Instance, false);
                        DeviceHelper.SetDeviceEnabled(DTTDE_GUID, DTTDE_Instance, false);
                    }
                }
                catch { }
            });
        }

        public static async void ApplyFix()
        {
            await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(25000);

                try
                {
                    if (MotherboardInfo.Product.Contains("Flow Z13") && Settings.Default.StartOnBoot == true)
                    {
                        DeviceHelper.SetDeviceEnabled(DLAHI_GUID, DLAHI_Instance, true);
                        DeviceHelper.SetDeviceEnabled(DTTDE_GUID, DTTDE_Instance, true);
                    }
                }
                catch { }
            });
        }

        private XgMobileConnectionService xgMobileConnectionService;

        private void SetUpXgMobileDetection()
        {
            xgMobileConnectionService.XgMobileStatus += (_, e) =>
            {
                if (e.DetectedChanged)
                {
                    ShowDetectedToast(e.Detected);
                }
                if (e.Connected)
                {
                    ToastNotification.HideXgMobileActivateToasts();
                }
            };
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                if (ToastNotification.IsActivateXgMobileToastButtonClicked(toastArgs))
                {
                    HandleXgMobileToast(true);
                }
                else if (ToastNotification.IsOpenXgMobileToastClicked(toastArgs))
                {
                    HandleXgMobileToast(false);
                }
            };
        }

        private void ShowDetectedToast(bool detected)
        {
            if (detected)
            {
                ToastNotification.PromptXgMobileActivate();
            }
            else
            {
                ToastNotification.HideXgMobileActivateToasts();
            }
        }

        private void HandleXgMobileToast(bool activate)
        {
            Dispatcher.Invoke(() =>
            {
                new XG_Mobile_Prompt(xgMobileConnectionService, activate).Show();
            });
        }
    }
}