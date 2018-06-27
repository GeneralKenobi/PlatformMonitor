using CSharpEnhanced.ICommands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace PlatformMonitor.Core
{
	/// <summary>
	/// View model for the app
	/// </summary>
	public class ViewModel : INotifyPropertyChanged
	{
		#region Constructor

		/// <summary>
		/// Default Constructor
		/// </summary>
		public ViewModel()
		{
			CreateNewServiceCommand = new RelayCommand(CreateNewService);
		}

		#endregion

		#region Property changed event

		/// <summary>
		/// Event raised when one of properties changes
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region ICommands

		/// <summary>
		/// Command initiating creation of a new service
		/// </summary>
		public ICommand CreateNewServiceCommand { get; set; }

		#endregion

		#region Public Properties

		/// <summary>
		/// Monitoring Manager assigned to this view model
		/// </summary>
		public Monitoring MonitoringManager { get; private set; } = new Monitoring();

		/// <summary>
		/// Url used when creating new service
		/// </summary>
		public string NewServiceUrl { get; set; } = "https://platforma.polsl.pl/rau3/";

		/// <summary>
		/// Name added to the new service
		/// </summary>
		public string NewServiceName { get; set; }

		/// <summary>
		/// Period to wait before the next check in the new service
		/// </summary>
		public int NewServicePeriod { get; set; } = Monitoring.DefaultPeriod;

		#endregion

		#region Private Methods

		/// <summary>
		/// Method for <see cref="CreateNewServiceCommand"/>
		/// </summary>
		private void CreateNewService()
		{
			// If the new name is empty don't create the new servce
			if (!string.IsNullOrWhiteSpace(NewServiceName))
			{
				MonitoringManager.CreateService(NewServiceUrl, NewServiceName, NewServicePeriod);

				NewServiceName = string.Empty;
			}
		}

		#endregion
	}
}