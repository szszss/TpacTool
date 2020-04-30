using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TpacTool
{
	public class IntThresholdConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int v = System.Convert.ToInt32(value);
			int threshold = int.Parse((string)parameter);
			if (targetType == typeof(Visibility) || targetType == typeof(Nullable<Visibility>))
				return v >= threshold ? Visibility.Visible : Visibility.Collapsed;
			return v >= threshold;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int v = System.Convert.ToInt32(value);
			int threshold = int.Parse((string)parameter);
			if (targetType == typeof(Visibility) || targetType == typeof(Nullable<Visibility>))
				return v >= threshold ? Visibility.Visible : Visibility.Collapsed;
			return v >= threshold;
		}
	}
}