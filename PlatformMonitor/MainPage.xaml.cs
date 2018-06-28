using Microsoft.Toolkit.Uwp.Notifications;
using PlatformMonitor.Core;
using System;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PlatformMonitor
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
    {
		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public MainPage()
        {
			this.InitializeComponent();

			// Create a new viewmodel
			var viewModel = new ViewModel();

			// Subscribe to its monitoring manager's name spotted event
			viewModel.MonitoringManager.NameSpotted += NameSpottedCallback;
			
			// And use it as a data context
			this.DataContext = viewModel;
        }

		#endregion

		#region Private methods

		/// <summary>
		/// Callback for when someone is spotted
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NameSpottedCallback(object sender, NameSpottedEventArgs e)
		{
			GenerateToast(e.Url, e.Name, e.Time);
		}

		/// <summary>
		/// Generates and sends a toast notification
		/// </summary>
		/// <param name="url"></param>
		/// <param name="name"></param>
		/// <param name="time"></param>
		private void GenerateToast(string url, string name, DateTime time)
		{
			string title = name + " was spotted!";
			string content = "Active on: " + url + " on " + time.ToLongDateString() + ", " + time.ToLongTimeString();

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

			// Construct the final toast content
			ToastContent toastContent = new ToastContent()
			{
				Visual = visual,
			};

			// And create the toast notification
			var toast = new ToastNotification(toastContent.GetXml());

			// If the same user appeared previously, remove the old notification
			ToastNotificationManager.History.Remove(name);

			// Set the expiration to now day
			toast.ExpirationTime = DateTime.Now.AddDays(1);

			// Set the name as the tag
			toast.Tag = name;

			// And the group name
			toast.Group = "PlatformMonitor";
			
			// Show the notification
			ToastNotificationManager.CreateToastNotifier().Show(toast);
		}

		#endregion
	}
}