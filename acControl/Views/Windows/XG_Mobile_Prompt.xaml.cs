using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace acControl.Views.Windows
{
    /// <summary>
    /// Interaction logic for XG_Mobile_Prompt.xaml
    /// </summary>
    public partial class XG_Mobile_Prompt : UiWindow
    {
        public XG_Mobile_Prompt()
        {
            InitializeComponent();

            UpdateImg(App.location + "\\Images\\XGMobile\\XGMobile-1.png");

            tbxInfo.Text = "Press \"Start Activation Process\" to begin the activation process of your ROG XG Mobile. \n\n\nWARNING: Do not attempt without an ROG XG Mobile!";
        }

        private void UpdateImg(string path)
        {
          imgDiagram.Source  = new BitmapImage(new Uri(path));
        }
    }
}
