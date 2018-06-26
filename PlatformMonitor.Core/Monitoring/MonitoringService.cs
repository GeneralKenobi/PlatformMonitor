using CSharpEnhanced.CoreClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlatformMonitor.Core
{
	/// <summary>
	/// Represents a service that checks presence on one given url.
	/// </summary>
    public class MonitoringService : INotifyPropertyChangedImplemented
    {
		#region Constructor

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="url">Url of the site to check (typically one of educational platforms)</param>
		/// <param name="name">Name of the person to look for</param>
		/// <param name="period">Period between refreshing, in seconds. Should fit between the min and max allowed value</param>
		public MonitoringService(string url, ObservableCollection<string> names, EventHandler<NameSpottedEventArgs> eventToRaise,
			int period = 30)
		{
			Url = url;
			Names = names;
			Period = CheckPeriodValue(period);
			EventToRaise = eventToRaise;
		}

		#endregion		

		#region Public Properties

		/// <summary>
		/// Seconds remaining until the next check
		/// </summary>
		public int SecondsToCheck = 0;

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

		#endregion

		#region Private properties

		/// <summary>
		/// Provides means for canelling an ongoing monitoring
		/// </summary>
		private CancellationTokenSource Cancellation { get; set; }

		/// <summary>
		/// Event to raise when someone was found on the platform
		/// </summary>
		private EventHandler<NameSpottedEventArgs> EventToRaise { get; set; }

		#endregion

		#region Public static properties

		/// <summary>
		/// The minimum value of period allowed
		/// </summary>
		public static int MinPeriod => 10;

		/// <summary>
		/// The maximum value of period allowed
		/// </summary>
		public static int MaxPeriod => 240;

		#endregion

		#region Public methods

		/// <summary>
		/// Starts the monitoring
		/// </summary>
		public void StartMonitoring()
		{
			IsRunning = true;
			MonitoringAsync();
		}

		/// <summary>
		/// Stops the monitoring
		/// </summary>
		public void StopMonitoring()
		{
			IsRunning = false;
			Cancellation?.Cancel();
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Task that monitors asynchronously the Url
		/// </summary>
		/// <returns></returns>
		private async Task MonitoringAsync()
		{
			// Create a new cancellation token source
			Cancellation = new CancellationTokenSource();
			
			// Keep going until cancellation is requested
			while (!Cancellation.IsCancellationRequested)
			{
				await CheckAndNotifyAsync();

				await WaitForAnotherCheck();
			}

			Cancellation.Dispose();
			Cancellation = null;
		}

		/// <summary>
		/// Ends when either the cancellation was requested or a full period has passed
		/// </summary>
		/// <returns></returns>
		private async Task WaitForAnotherCheck()
		{			
			// Get the current time
			DateTime now = DateTime.Now;

			// Compute the time of the next check
			DateTime nextCheck = now.AddSeconds(Period);

			// Create a wait handle
			AutoResetEvent waitHandle = new AutoResetEvent(false);

			// Create a callback for the timer
			TimerCallback timerCallback = (x) =>
			{
				// Calculate the number of remaining seconds (use Max with 0 so as not to get a negative time by accident)
				int remainingTime = (int)Math.Max(Math.Round((nextCheck - DateTime.Now).TotalSeconds), 0);

				// Assign it to the public property
				SecondsToCheck = remainingTime;

				// If the period has passed or cancellation was requested
				if (remainingTime <= 0 || Cancellation.IsCancellationRequested)
				{
					// Set the wait handle
					waitHandle.Set();
				}
			};

			// Create the timer
			Timer timer = new Timer(timerCallback, null, 0, 1000);

			// And wait for the callback to notify that the waiting is over
			waitHandle.WaitOne();

			// Finally dispose of the timer
			timer.Dispose();
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
		private void Notify(string name) => EventToRaise.Invoke(this, new NameSpottedEventArgs(Url, name, DateTime.Now));

		/// <summary>
		/// Checks if the period falls withing the allowed range and returns it. If not, the closest correct value will be returned.
		/// </summary>
		/// <param name="period"></param>
		/// <returns></returns>
		private int CheckPeriodValue(int period)
		{
			if (period < MinPeriod)
			{
				return MinPeriod;
			}

			if(period > MaxPeriod)
			{
				return MaxPeriod;
			}

			return period;
		}

		#endregion		
	}
}