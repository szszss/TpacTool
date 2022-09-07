using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace TpacTool
{
	/// <summary>
	/// WpfPreviewPage.xaml 的交互逻辑
	/// </summary>
	public partial class WpfPreviewPage : Page
	{
		public static readonly DependencyProperty CameraTargetProperty =
			DependencyProperty.Register(
				"CameraTarget", typeof(Point3D),
				typeof(WpfPreviewPage),
				new UIPropertyMetadata(new Point3D(), (o, args) =>
					{
						var viewport = ((WpfPreviewPage)o).Viewport3D;
						if (viewport.CameraController != null)
						{
							var pos = viewport.CameraController.CameraPosition;
							var oldTarget = (Point3D)args.OldValue;
							var newTarget = (Point3D)args.NewValue;
							pos.Offset(newTarget.X - oldTarget.X, newTarget.Y - oldTarget.Y, newTarget.Z - oldTarget.Z);
							viewport.CameraController.CameraPosition = pos;
						}
					})
			);

		public static readonly DependencyProperty ReferenceScaleProperty =
			DependencyProperty.Register(
				"ReferenceScale", typeof(double),
				typeof(WpfPreviewPage),
				new UIPropertyMetadata(1d, (o, args) =>
				{
					var page = (WpfPreviewPage)o;
					var viewport = page.Viewport3D;
					if (viewport.CameraController != null)
						page.ApplyCameraDistance((double)args.NewValue, (double)args.OldValue);
				})
			);

		public static readonly DependencyProperty KeepScaleModeProperty =
			DependencyProperty.Register(
				"KeepScaleMode", typeof(bool),
				typeof(WpfPreviewPage),
				new PropertyMetadata(true)
			);

		public Point3D CameraTarget
		{
			set { SetValue(CameraTargetProperty, value); }
			get { return Viewport3D.CameraController.CameraTarget; }
		}

		public double ReferenceScale
		{
			set { SetValue(ReferenceScaleProperty, Math.Max(value, 0.001)); }
			get { return (double) GetValue(ReferenceScaleProperty); }
		}

		public bool KeepScaleMode
		{
			set { SetValue(KeepScaleModeProperty, value); }
			get { return (bool)GetValue(KeepScaleModeProperty); }
		}

		private void ApplyCameraDistance(double newScale, double oldScale)
		{
			if (KeepScaleMode)
			{
				var cc = Viewport3D.CameraController;
				var camTarget = cc.CameraTarget;
				var oldVec = cc.CameraLookDirection;
				var oldLength = oldVec.Length;
				oldVec.Normalize();
				var newLength = oldLength * newScale / oldScale;
				var newVec = Vector3D.Multiply(oldVec, newLength);
				cc.CameraLookDirection = newVec;
				cc.CameraPosition = Point3D.Subtract(camTarget, newVec);
			}
		}

		public WpfPreviewPage()
		{
			InitializeComponent();
			SetBinding(CameraTargetProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath("CameraTarget"),
				Mode = BindingMode.OneWay
			});
			SetBinding(ReferenceScaleProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath("ReferenceScale"),
				Mode = BindingMode.OneWay
			});
			SetBinding(KeepScaleModeProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath("KeepScaleMode"),
				Mode = BindingMode.OneWay
			});
			Viewport3D.Loaded += (sender, args) =>
			{
				Viewport3D.CameraController.CameraLookDirection = new Vector3D(-1, 0, 0);
				ApplyCameraDistance((double) GetValue(ReferenceScaleProperty), 
									(double) GetValue(ReferenceScaleProperty));
				Viewport3D.CameraController.CameraTarget = (Point3D) GetValue(CameraTargetProperty);
			};
		}
	}
}
