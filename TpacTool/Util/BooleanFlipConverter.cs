using System;
using System.Globalization;
using System.Windows.Data;

namespace TpacTool
{
	public class BooleanFlipConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof(bool) && targetType != typeof(Nullable<bool>))
				throw new ArgumentException("The value must be a boolean");
			return !((bool) value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof(bool) && targetType != typeof(Nullable<bool>))
				throw new ArgumentException("The value must be a boolean");
			return !((bool)value);
		}
	}
}