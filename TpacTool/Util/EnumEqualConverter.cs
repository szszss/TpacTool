using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TpacTool
{
	public class EnumEqualConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && parameter != null && value.GetType().IsEnum)
				return value.Equals(parameter);
			return DependencyProperty.UnsetValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool && (bool)value)
				return parameter;
			return DependencyProperty.UnsetValue;
		}
	}
}