using System;
using Windows.UI.Xaml.Data;

namespace PlatformMonitor
{
	/// <summary>
	/// If value is a bool return a play icon for false and pause icon for true. Otherwise returns <see cref="string.Empty"/>
	/// </summary>
	public class BoolToPauseIconConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if(value is bool b)
			{
				return b ? "\uE769" : "\uE768";
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
