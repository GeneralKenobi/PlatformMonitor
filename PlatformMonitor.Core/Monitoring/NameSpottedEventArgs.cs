using System;

namespace PlatformMonitor.Core
{
	/// <summary>
	/// Arguments for an event notifying that someone was spotted
	/// </summary>
	public class NameSpottedEventArgs : EventArgs
    {
		#region Public properties

		/// <summary>
		/// Url on which the target was spotted
		/// </summary>
		public string Url { get; private set; }

		/// <summary>
		/// Name of the target
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Time of spotting
		/// </summary>
		public DateTime Time { get; private set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Default Constructor
		/// </summary>
		public NameSpottedEventArgs(string url, string name, DateTime time)
		{
			Url = url;
			Name = name;
			Time = time;
		}

		#endregion
	}
}