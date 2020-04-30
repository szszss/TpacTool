using System;
using System.Collections.Generic;
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
using TabControl = BetterWpfControls.TabControl;
using TabItem = BetterWpfControls.TabItem;

namespace TpacTool
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void TabControl_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			TabControl tabControl = sender as TabControl;
			/*if (tabControl == null)
			{
				TabItem tabItem = sender as TabItem;
				if (tabItem != null)
				{
					tabControl = tabItem.Parent as TabControl;
				}
			}*/
			if (tabControl != null && tabControl.Items.Count > 1)
			{
				bool hover = false;
				foreach (TabItem item in tabControl.Items)
				{
					hover |= item.IsMouseOver;
				}
				if (hover && e.Delta > 0 && tabControl.SelectedIndex > 0)
				{
					tabControl.SelectedIndex = tabControl.SelectedIndex - 1;
				}
				else if (hover && e.Delta < 0 && tabControl.SelectedIndex < tabControl.Items.Count - 1)
				{
					tabControl.SelectedIndex = tabControl.SelectedIndex + 1;
				}
			}
		}
	}
}
