using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;

namespace PlatformMonitor.Core
{
    public class Monitoring
    {
		#region Public properties
		
		/// <summary>
		/// List with all currently managed <see cref="MonitoringService"/>s
		/// </summary>
		public List<MonitoringService> ManagedServices { get; private set; } = new List<MonitoringService>();

		/// <summary>
		/// Event raised when one of the managed <see cref="MonitoringService"/>s finds one of its targets
		/// </summary>
		public EventHandler<NameSpottedEventArgs> NameSpotted;

		#endregion

		#region Public methods

		/// <summary>
		/// Creates a new service for monitoring
		/// </summary>
		/// <param name="url"></param>
		/// <param name="name"></param>
		/// <param name="period"></param>
		public void CreateService(string url, string name, int period)
		{
			ManagedServices.Add(new MonitoringService(url, new ObservableCollection<string>() { url }, NameSpotted, 10));
		}

		#endregion		
	}
}
