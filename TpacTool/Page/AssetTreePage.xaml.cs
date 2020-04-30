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
using LiteTreeView;

namespace TpacTool
{
    /// <summary>
    /// AssetTreePage.xaml 的交互逻辑
    /// </summary>
    public partial class AssetTreePage : Page
    {
        public AssetTreePage()
        {
            InitializeComponent();
        }

		private void LiteTreeViewControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var vm = DataContext as AssetTreeViewModel;
			if (vm != null && e.AddedItems.Count > 0)
			{
				var obj = e.AddedItems[0] as LiteTreeViewItemViewModel;
				var asset = obj.InnerObject as AssetTreeViewModel.AssetTreeNode;
				if (asset != null)
				{
					vm.SelectAsset(asset.Asset);
				}
			}
		}
	}
}
