using CSharpEnhanced.CoreClasses;
using CSharpEnhanced.ICommands;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlatformMonitor.Core
{
	/// <summary>
	/// Represents a service that checks presence on one given url.
	/// </summary>
    public class MonitoringService : INotifyPropertyChanged
    {
		#region Property changed event

		/// <summary>
		/// Event raised whenever one of proprties changes
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Constructor

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="url">Url of the site to check (typically one of educational platforms)</param>
		/// <param name="name">Name of the person to look for</param>
		/// <param name="period">Period between refreshing, in seconds. Should fit between the min and max allowed value</param>
		public MonitoringService(string url, ObservableCollection<string> names, ICommand removeServiceCommand,
			int period = 30)
		{
			Url = url;
			Names = names;
			RemoveServiceCommand = removeServiceCommand;
			Period = CheckPeriodValue(period);

			AddNewNameCommand = new RelayCommand(AddNewName);
			RemoveNameCommand = new RelayParametrizedCommand(RemoveName);
			PauseCommand = new RelayCommand(Pause);
		}
		
		#endregion		

		#region Public Properties

		/// <summary>
		/// Seconds remaining until the next check
		/// </summary>
		public int SecondsToCheck { get; private set; }

		/// <summary>
		/// Indicates whether this service is currently running
		/// </summary>
		public bool IsRunning { get; private set; } = false;

		/// <summary>
		/// Url of the site to check (typically one of educational platforms)
		/// </summary>
		public string Url { get; private set; }

		/// <summary>
		/// Name of the person to look for
		/// </summary>
		public ObservableCollection<string> Names { get; private set; }

		/// <summary>
		/// Period between refreshing, in seconds
		/// </summary>
		public int Period { get; private set; }

		/// <summary>
		/// Name to add when <see cref="AddNewNameCommand"/> is invoked
		/// </summary>
		public string NewName { get; set; }

		#endregion

		#region Commands

		/// <summary>
		/// Adds a new name to the list of the searched names
		/// </summary>
		public ICommand AddNewNameCommand { get; private set; }

		/// <summary>
		/// Removes a name (passed parameter) from the list of the searched commands
		/// </summary>
		public ICommand RemoveNameCommand { get; private set; }

		/// <summary>
		/// Pauses or resumes this service
		/// </summary>
		public ICommand PauseCommand { get; private set; }

		/// <summary>
		/// Removes this service (which should be the parameter) from the owning <see cref="Monitoring"/>
		/// </summary>
		public ICommand RemoveServiceCommand { get; private set; }

		#endregion

		#region Private properties

		/// <summary>
		/// Provides means for cancelling an ongoing monitoring
		/// </summary>
		private CancellationTokenSource Cancellation { get; set; }

		/// <summary>
		/// Event to raise when someone was found on the platform
		/// </summary>
		public EventHandler<NameSpottedEventArgs> NameSpotted;

		#endregion		

		#region Public methods

		/// <summary>
		/// Starts the monitoring
		/// </summary>
		public void StartMonitoring()
		{
			if (!IsRunning)
			{
				MonitoringAsync();
			}
		}

		/// <summary>
		/// Stops the monitoring
		/// </summary>
		public void StopMonitoring()
		{
			if (IsRunning)
			{
				SecondsToCheck = Period;
				Cancellation?.Cancel();
			}
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Method for <see cref="PauseCommand"/>
		/// </summary>
		private void Pause()
		{
			if (IsRunning)
			{
				StopMonitoring();
			}
			else
			{
				StartMonitoring();
			}
		}

		/// <summary>
		/// Method for <see cref="RemoveNameCommand"/>
		/// </summary>
		/// <param name="parameter"></param>
		private void RemoveName(object parameter)
		{
			if(parameter is string name)
			{
				Names.Remove(name);
			}
		}

		/// <summary>
		/// Method for <see cref="AddNewNameCommand"/>
		/// </summary>
		private void AddNewName()
		{
			if(!string.IsNullOrWhiteSpace(NewName))
			{
				Names.Add(NewName);
			}

			NewName = string.Empty;
		}

		/// <summary>
		/// Task that monitors asynchronously the Url
		/// </summary>
		/// <returns></returns>
		private async Task MonitoringAsync()
		{
			IsRunning = true;
			SecondsToCheck = Period;

			// Create a new cancellation token source
			Cancellation = new CancellationTokenSource();
			
			// Keep going until cancellation is requested
			while (!Cancellation.IsCancellationRequested)
			{
				await CheckAndNotifyAsync();

				await WaitForAnotherCheck();
			}

			IsRunning = false;
			SecondsToCheck = Period;

			Cancellation.Dispose();
			Cancellation = null;
		}

		/// <summary>
		/// Ends when either the cancellation was requested or a full period has passed
		/// </summary>
		/// <returns></returns>
		private async Task WaitForAnotherCheck()
		{
			int remainingSeconds = Period;			

			while(remainingSeconds > 1 && !Cancellation.IsCancellationRequested)
			{
				--remainingSeconds;
				SecondsToCheck = remainingSeconds;

				await Task.Delay(1000);
			}			
		}

		/// <summary>
		/// Checks whether given names are present on the website and notify for those that are
		/// </summary>
		/// <returns></returns>
		private async Task CheckAndNotifyAsync()
		{
			// Create a client to access the website
			var client = new HttpClient();

			string siteContent = string.Empty;
			try
			{
				// Get the website and store it's html in a string (the recently logged are written in it)
				siteContent = await (await client.GetAsync(Url)).Content.ReadAsStringAsync();
			}
			// TODO: Add exception handling
			catch (Exception e) { }

			// Check if any names come up in the html
			foreach (var name in Names)
			{
				if (siteContent.Contains(name))
				{
					// If so, notify
					Notify(name);
				}
			}
		}

		/// <summary>
		/// Notifies the <see cref="Monitoring"/>
		/// </summary>
		/// <param name="name"></param>
		private void Notify(string name) => NameSpotted?.Invoke(this, new NameSpottedEventArgs(Url, name, DateTime.Now));
		

		/// <summary>
		/// Checks if the period falls withing the allowed range and returns it. If not, the closest correct value will be returned.
		/// </summary>
		/// <param name="period"></param>
		/// <returns></returns>
		private int CheckPeriodValue(int period)
		{
			if (period < Monitoring.MinPeriod)
			{
				return Monitoring.MinPeriod;
			}

			if(period > Monitoring.MaxPeriod)
			{
				return Monitoring.MaxPeriod;
			}

			return period;
		}

		#endregion		
	}
}