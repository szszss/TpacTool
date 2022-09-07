using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using TpacTool.Lib;

namespace TpacTool
{
	public sealed class WpfPreviewViewModel : AbstractPreviewViewModel
	{
		private Uri _pageUri = new Uri("../Page/WpfPreviewPage.xaml", UriKind.Relative);

		public override Uri PageUri => _pageUri;

		public Model3DGroup Models { private set; get; } = new Model3DGroup();

		public LightSetup Light { private set; get; } = new SunLight();

		public Vector3D GridNormal { private set; get; } = new Vector3D(0, 0, -1);

		public Point3D GridCenter { private set; get; } = new Point3D(0, 0, 0);

		public WpfPreviewViewModel() : base()
		{
			if (!IsInDesignMode)
			{
				UpdateLights();
				MessengerInstance.Register<object>(this, MainViewModel.CleanupEvent, OnCleanup);
			}
		}

		private void OnCleanup(object unused = null)
		{
			Models.Children.Clear();
			RaisePropertyChanged("Models");
		}

		public override void SetRenderMeshes(params Mesh[] meshes)
		{
			Models.Children.Clear();
			bool firstMesh = true;
			BoundingBox bb = new BoundingBox();
			CenterOfMass = new Point3D();
			CenterOfGeometry = new Point3D();
			foreach (var mesh in meshes)
			{
				var bakedMesh = ResourceCache.GetModel(mesh);
				Models.Children.Add(bakedMesh.Mesh);
				CenterOfMass = Point3D.Add(CenterOfMass, bakedMesh.CenterOfMass.ToVector3D());
				if (firstMesh)
				{
					bb = bakedMesh.BoundingBox;
					firstMesh = false;
				}
				else
				{
					bb = BoundingBox.Merge(bb, bakedMesh.BoundingBox);
				}
				//bb = BoundingBox.Merge(bb, mesh.BoundingBox.ToSharpDxBoundingBox());
			}

			if (meshes.Length > 0)
			{
				CenterOfMass = new Point3D(
					CenterOfMass.X / meshes.Length,
					CenterOfMass.Y / meshes.Length,
					CenterOfMass.Z / meshes.Length);
			}

			ClampBoundingBox(ref bb);
			ModelBoundingBox = bb;
			var center = ModelBoundingBox.Center;
			CenterOfGeometry = new Point3D(center.X, center.Y, center.Z);
			ReferenceScale = CalculateReferenceScale(ModelBoundingBox);
			//UpdateGridLines(bb);

			RaisePropertyChanged(nameof(Models));
			RefocusCenter();
			RaisePropertyChanged(nameof(ReferenceScale));
		}

		protected override void UpdateLights()
		{
			switch (LightMode)
			{
				case 1:
					Light = new ThreePointLights();
					break;
				case 2:
					Light = new DefaultLights();
					break;
				default:
					Light = new SunLight();
					break;
			}
			RaisePropertyChanged("Light");
		}

		private void UpdateGridLines(BoundingBox bb)
		{
			GridWidth = bb.Width + 16;
			GridLength = bb.Depth + 16;
			RaisePropertyChanged("GridWidth");
			RaisePropertyChanged("GridLength");
		}
	}
}