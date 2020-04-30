using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TpacTool.Lib;

namespace TpacTool
{
	/// <summary>
	/// AboutWindow.xaml 的交互逻辑
	/// </summary>
	public partial class AboutWindow : Window
	{
		public string VersionExe => typeof(App).Assembly.GetName().Version.ToString(3);

		public string VersionLib => typeof(AssetPackage).Assembly.GetName().Version.ToString(3);

		public AboutWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
	}
}
