using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
	/// Vec4Box.xaml 的交互逻辑
	/// </summary>
	// not finish yet. write back is not supported
	public partial class Vec4Box : UserControl
	{
		private bool _isReadOnly;
		private Vector4 _value;

		public bool IsReadOnly
		{
			set
			{
				_isReadOnly = value;
				CompX.IsReadOnly = value;
				CompY.IsReadOnly = value;
				CompZ.IsReadOnly = value;
				CompW.IsReadOnly = value;
			}
			get => _isReadOnly;
		}

		public Vector4 Value
		{
			set
			{
				_value = value;
				CompX.Text = String.Format("{0}", Value.X);
				CompY.Text = String.Format("{0}", Value.Y);
				CompZ.Text = String.Format("{0}", Value.Z);
				CompW.Text = String.Format("{0}", Value.W);
			}
			get => _value;
		}

		public Vec4Box()
		{
			InitializeComponent();
		}
	}
}
