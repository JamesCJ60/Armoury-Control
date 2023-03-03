using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace acControl.Scripts
{
    class ToastNotification
    {
        public static void ShowToastNotification(string title, string body)
        {
            var iconUri = "file:///" + App.location +"Assets\\applicationIcon.png";

            new ToastContentBuilder()
                .AddText(title)
                .AddText(body)
                .AddAppLogoOverride(new Uri(iconUri))
                .Show();
        }
    }
}
