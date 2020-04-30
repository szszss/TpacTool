using System;
using System.Globalization;
using System.Windows.Data;

namespace TpacTool
{
	public class ArrayGetterConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((Array) value).GetValue(int.Parse((string)parameter));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((Array)value).GetValue(int.Parse((string)parameter));
		}
	}
}