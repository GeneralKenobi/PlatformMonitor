using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace PlatformMonitor.Core
{
    public class Monitoring
    {
		#region Public properties

		public List<MonitoringService> ManagedServices { get; private set; } = new List<MonitoringService>();

		#endregion

		#region Private methods

		private async void Monitor()
		{
			
		}

		#endregion

		#region Public methods

		public void CreateService(string url, string name, int period)
		{

		}

		#endregion

		
	}
}
