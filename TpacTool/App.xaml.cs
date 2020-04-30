using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using TpacTool.Properties;

namespace TpacTool
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
		private static CultureInfo DefaultCulture;

		private static CultureInfo DefaultUICulture;

		static App()
		{
			DispatcherHelper.Initialize();
			if (Settings.Default.RecentWorkDirs == null)
				Settings.Default.RecentWorkDirs = new StringCollection();

			DefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
			DefaultUICulture = CultureInfo.DefaultThreadCurrentUICulture;
			var lang = Settings.Default.Language;
			if (lang != null && lang.Length > 0 && !lang.Equals("default", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					CultureInfo ci = new CultureInfo(lang);
					CultureInfo.DefaultThreadCurrentCulture = ci;
					CultureInfo.DefaultThreadCurrentUICulture = ci;
				}
				catch (CultureNotFoundException)
				{
					Settings.Default.Language = "default";
					Settings.Default.Save();
				}
			}
			else if (lang != "default")
			{
				Settings.Default.Language = "default";
				Settings.Default.Save();
			}
		}

		public static void SetLanguage(string culture)
		{
			try
			{
				if (culture != null && culture.Length > 0 && !culture.Equals("default", StringComparison.OrdinalIgnoreCase))
				{
					CultureInfo ci = new CultureInfo(culture);
					CultureInfo.DefaultThreadCurrentCulture = ci;
					CultureInfo.DefaultThreadCurrentUICulture = ci;
					Settings.Default.Language = culture;
					Settings.Default.Save();
				}
				else
				{
					Settings.Default.Language = "default";
					Settings.Default.Save();
					CultureInfo.DefaultThreadCurrentCulture = DefaultCulture;
					CultureInfo.DefaultThreadCurrentUICulture = DefaultUICulture;
				}
			}
			catch (CultureNotFoundException)
			{
				Settings.Default.Language = "default";
				Settings.Default.Save();
				CultureInfo.DefaultThreadCurrentCulture = DefaultCulture;
				CultureInfo.DefaultThreadCurrentUICulture = DefaultUICulture;
			}
		}
	}
}
