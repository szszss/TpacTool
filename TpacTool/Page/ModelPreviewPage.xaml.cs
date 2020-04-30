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

namespace TpacTool
{
	/// <summary>
	/// ModelPreviewPage.xaml 的交互逻辑
	/// </summary>
	public partial class ModelPreviewPage : Page
	{
		private bool isDragging = false;
		private Point lastPos;

		public ModelPreviewPage()
		{
			InitializeComponent();
		}

		private void Viewport3D_MouseMove(object sender, MouseEventArgs e)
		{
			if (isDragging)
			{
				var vm = DataContext as ModelPreviewViewModel;
				if (vm != null)
				{
					var curPos = e.GetPosition(this);
					vm.UpdateMouse(curPos.X - lastPos.X, curPos.Y - lastPos.Y, 0);
					lastPos = curPos;
				}
			}
		}

		private void Viewport3D_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			var vm = DataContext as ModelPreviewViewModel;
			if (vm != null)
			{
				vm.UpdateMouse(0, 0, e.Delta);
			}
		}

		private void Viewport3D_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			isDragging = true;
			lastPos = e.GetPosition(this);
		}

		private void Viewport3D_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			isDragging = false;
		}

		private void Viewport3D_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			var vm = DataContext as ModelPreviewViewModel;
			if (vm != null)
			{
				vm.UpdateProjectionMatrix(e.NewSize.Width / e.NewSize.Height);
			}
		}

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ModelPreviewViewModel;
			if (vm != null)
			{
				vm.UpdateProjectionMatrix(Canvas.RenderSize.Width / Canvas.RenderSize.Height);
			}
		}
	}
}
