using System;
using CSharpEnhanced.CoreClasses;

namespace PlatformMonitor.Core
{
	/// <summary>
	/// ViewModel for the application
	/// </summary>
    public class ViewModel : INotifyPropertyChangedImplemented
    {
		#region Public properties

		/// <summary>
		/// Monitoring used in the given session
		/// </summary>
		public Monitoring MonitoringManager { get; private set; } = new Monitoring();

		#endregion
	}
}
