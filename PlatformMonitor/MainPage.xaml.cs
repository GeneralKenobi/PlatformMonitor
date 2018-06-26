using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using PlatformMonitor.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PlatformMonitor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
			this.InitializeComponent();
        }

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			string title = "Activity on the platform!";
			string content = "Filus spotted";

			// Construct the visuals of the toast
			ToastVisual visual = new ToastVisual()
			{
				BindingGeneric = new ToastBindingGeneric()
				{
					Children =
					{
						new AdaptiveText()
						{
							Text = title
						},

						new AdaptiveText()
						{
							Text = content
						},
					},
				}
			};

			// Now we can construct the final toast content
			ToastContent toastContent = new ToastContent()
			{
				Visual = visual,
			};

			// And create the toast notification
			var toast = new ToastNotification(toastContent.GetXml());

			toast.ExpirationTime = DateTime.Now.AddDays(1);
			toast.Tag = DateTime.Now.ToShortDateString();
			toast.Group = "PlatformMonitor";

			ToastNotificationManager.CreateToastNotifier().Show(toast);
		}
	}
}
