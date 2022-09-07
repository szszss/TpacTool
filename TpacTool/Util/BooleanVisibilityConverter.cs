using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TpacTool
{
	public class BooleanVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool v = System.Convert.ToBoolean(value);
			bool shouldCollapsed = parameter != null && System.Convert.ToInt32(parameter) == 1;
			return v ? Visibility.Visible : shouldCollapsed ? Visibility.Collapsed : Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool v = System.Convert.ToBoolean(value);
			bool shouldCollapsed = parameter != null && System.Convert.ToInt32(parameter) == 1;
			return v ? Visibility.Visible : shouldCollapsed ? Visibility.Collapsed : Visibility.Hidden;
		}
	}
}