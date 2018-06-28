using CSharpEnhanced.ICommands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Foundation;

namespace PlatformMonitor.Core
{
	/// <summary>
	/// Container for <see cref="MonitoringService"/>s, propagates all events and creates a Log
	/// </summary>
	public class Monitoring : INotifyPropertyChanged
	{
		#region Events

		/// <summary>
		/// Invoked whenever a property changes
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Event raised when one of the managed <see cref="MonitoringService"/>s finds one of its targets
		/// </summary>
		public EventHandler<NameSpottedEventArgs> NameSpotted;

		#endregion

		#region Constructor

		/// <summary>
		/// Default Constructor
		/// </summary>
		public Monitoring()
		{
			ManagedServices = new ReadOnlyObservableCollection<MonitoringService>(_ManagedServices);
			Log = new ReadOnlyObservableCollection<string>(_Log);

			RemoveServiceCommand = new RelayParametrizedCommand(RemoveService);

			RequestExtendedSession();
		}

		#endregion

		#region Commands

		/// <summary>
		/// Removes a <see cref="MonitoringService"/> given by parameter from the <see cref="ManagedServices"/> collection
		/// </summary>
		public ICommand RemoveServiceCommand { get; set; }

		#endregion

		#region Private members

		/// <summary>
		/// Session used to ensure that when the app is minimized it won't be suspended
		/// </summary>
		private ExtendedExecutionSession _Session;

		/// <summary>
		/// Backign store for <see cref="ManagedServices"/>
		/// </summary>
		private ObservableCollection<MonitoringService> _ManagedServices = new ObservableCollection<MonitoringService>();

		/// <summary>
		/// Backing store for <see cref="Log"/>
		/// </summary>
		private ObservableCollection<string> _Log = new ObservableCollection<string>();

		#endregion

		#region Public properties

		/// <summary>
		/// Collection with all currently handled <see cref="MonitoringService"/>s
		/// </summary>
		public ReadOnlyObservableCollection<MonitoringService> ManagedServices { get; set; }

		/// <summary>
		/// Collection of strings representing log entries with information about spotting of a target
		/// </summary>
		public ReadOnlyObservableCollection<string> Log { get; set; }


		#endregion

		#region Public static properties

		/// <summary>
		/// The minimum value of period allowed
		/// </summary>
		public static int MinPeriod => 10;

		/// <summary>
		/// The default period
		/// </summary>
		public static int DefaultPeriod => 30;

		/// <summary>
		/// The maximum value of period allowed
		/// </summary>
		public static int MaxPeriod => 240;

		#endregion

		#region Private methods

		/// <summary>
		/// Requests an extended session from the OS so as not to be suspended
		/// </summary>
		private void RequestExtendedSession()
		{
			
			_Session = new ExtendedExecutionSession();

			_Session.Reason = ExtendedExecutionReason.Unspecified;
			_Session.Description = "Periodic check of the website";

			_Session.RequestExtensionAsync();
			
		}

		/// <summary>
		/// Method for <see cref="RemoveServiceCommand"/>
		/// </summary>
		/// <param name="parameter"></param>
		private void RemoveService(object parameter)
		{
			if(parameter is MonitoringService service)
			{
				service.StopMonitoring();
				_ManagedServices.Remove(service);
			}

		}

		/// <summary>
		/// Propagates an event raised in a <see cref="MonitoringService"/> from <see cref="_ManagedServices"/> and adds its info
		/// to the log (<see cref="_Log"/>)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PropagateEvent(object sender, NameSpottedEventArgs e)
		{
			_Log.Add(e.Time.ToLongDateString() + ", " + e.Time.ToLongTimeString() + "\t" + e.Name + "\tspotted on " + e.Url);
			NameSpotted?.Invoke(sender, e);
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Creates a new service for monitoring and starts it
		/// </summary>
		/// <param name="url"></param>
		/// <param name="name"></param>
		/// <param name="period"></param>
		public void CreateService(string url, string name, int period)
		{
			var newAddition = new MonitoringService(url, new ObservableCollection<string>() { name }, RemoveServiceCommand, period);

			newAddition.NameSpotted += PropagateEvent;

			_ManagedServices.Add(newAddition);

			newAddition.StartMonitoring();
		}
		
		#endregion
	}
}