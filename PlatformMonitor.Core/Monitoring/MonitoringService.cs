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
		public MonitoringService(string url, ObservableCollection<string> names, int period = 30)
		{
			Url = url;
			Names = names;
			Period = CheckPeriodValue(period);
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
			var client = new HttpClient();

			while (!Cancellation.IsCancellationRequested)
			{
				var siteContent = await (await client.GetAsync(Url)).Content.ReadAsStringAsync();

				foreach (var name in Names)
				{
					if (siteContent.Contains(name))
					{
						Notify(name);
					}
				}

				DateTime now = DateTime.Now;
				DateTime nextCheck = now.AddSeconds(Period);

				AutoResetEvent waitHandle = new AutoResetEvent(false);

				TimerCallback timerCallback = (x) =>
				{
					int remainingTime = (int)Math.Max(Math.Round((nextCheck - DateTime.Now).TotalSeconds), 0);
					SecondsToCheck = remainingTime;

					if(remainingTime <= 0 || Cancellation.IsCancellationRequested)
					{
						waitHandle.Set();
					}
				};

				Timer t = new Timer(timerCallback, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));

				waitHandle.WaitOne();

				t.Dispose();
			}

			Cancellation.Dispose();
			Cancellation = null;
		}

		private void Notify(string name)
		{

		}

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
