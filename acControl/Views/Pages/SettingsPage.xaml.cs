using acControl.Properties;
using Microsoft.Win32.TaskScheduler;
using System.Linq;
using Wpf.Ui.Common.Interfaces;

namespace acControl.Views.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : INavigableView<ViewModels.SettingsViewModel>
    {
        public ViewModels.SettingsViewModel ViewModel
        {
            get;
        }

        public SettingsPage(ViewModels.SettingsViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            cbStart.IsChecked = Settings.Default.StartOnBoot;
            cbMini.IsChecked = Settings.Default.StartMini;
        }

        private void cbStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings.Default.StartOnBoot = (bool)cbStart.IsChecked;
            Settings.Default.Save();
            if ((bool)cbStart.IsChecked) updateTS();
            else deleteTS();
        }

        private void cbMini_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings.Default.StartMini = (bool)cbMini.IsChecked;
            Settings.Default.Save();
        }

        private void updateTS()
        {
            // Get the service on the local machine
            using (TaskService ts = new TaskService())
            {
                if (!ts.RootFolder.AllTasks.Any(t => t.Name == "Armoury Control"))
                {
                    // Create a new task definition and assign properties
                    TaskDefinition td = ts.NewTask();
                    td.Principal.RunLevel = TaskRunLevel.Highest;
                    td.RegistrationInfo.Description = "Start Armoury Control";

                    // Create a trigger that will fire the task at this time every other day
                    td.Triggers.Add(new LogonTrigger());

                    // Create an action that will launch Notepad whenever the trigger fires
                    string path = System.Reflection.Assembly.GetEntryAssembly().Location;
                    path = path.Replace("Armoury Control.dll", "Armoury Control.exe");
                    td.Actions.Add(path);

                    // Register the task in the root folder
                    ts.RootFolder.RegisterTaskDefinition(@"Armoury Control", td);
                }

            }
        }

        private void deleteTS() 
        {
            using (TaskService ts = new TaskService())
            {
                if (ts.RootFolder.AllTasks.Any(t => t.Name == "Armoury Control"))
                {
                    // Remove the task we just created
                    ts.RootFolder.DeleteTask("Armoury Control");
                }

                if (ts.RootFolder.AllTasks.Any(t => t.Name == "Start Armoury Control"))
                {
                    // Remove the task we just created
                    ts.RootFolder.DeleteTask("Start Armoury Control");
                }
            }
        }
    }
}