using System;
using System.Globalization;
using System.Windows.Data;

namespace TpacTool
{
	public class FloatEqualConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var v = System.Convert.ToSingle(value);
			var target = System.Convert.ToSingle(parameter);
			return Math.Abs(target - v) < 1e-5f;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var v = System.Convert.ToSingle(value);
			var target = System.Convert.ToSingle(parameter);
			return Math.Abs(target - v) < 1e-5f;
		}
	}
}