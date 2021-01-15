using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using SharpDX;
using TpacTool.Lib;
using BoundingBox = SharpDX.BoundingBox;

namespace TpacTool
{
	public class WpfPreviewViewModel : AbstractPreviewViewModel
	{
		private Uri _pageUri = new Uri("../Page/WpfPreviewPage.xaml", UriKind.Relative);

		private BoundingBox _modelBoundingBox = new BoundingBox();

		private Point3D _centerOfMass = new Point3D();

		private Point3D _centerOfGeometry = new Point3D();

		public override Uri PageUri => _pageUri;

		public Model3DGroup Models { private set; get; } = new Model3DGroup();

		public LightSetup Light { private set; get; } = new SunLight();

		public Vector3D GridNormal { private set; get; } = new Vector3D(0, 0, -1);

		public Point3D GridCenter { private set; get; } = new Point3D(0, 0, 0);

		public Point3D CameraTarget { private set; get; } = new Point3D(0, 0, 0);

		public double ReferenceScale { private set; get; } = 1;

		public WpfPreviewViewModel() : base()
		{
			if (!IsInDesignMode)
			{
				UpdateLights();
				MessengerInstance.Register<object>(this, MainViewModel.CleanupEvent, OnCleanup);
			}
		}

		protected override void OnPreviewModel(List<Mesh> meshes)
		{
			// TODO: a better fix. use edit data rather than vertex stream
			try
			{
				SetRenderMeshes(meshes.ToArray());
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				SetRenderMeshes();
			}
		}

		protected void OnCleanup(object unused = null)
		{
			Models.Children.Clear();
			RaisePropertyChanged("Models");
		}

		public void SetRenderMetamesh(Metamesh metamesh, int lod = 0)
		{
			try
			{
				var meshes = metamesh.Meshes.FindAll(mesh => { return mesh.Lod == lod; });
				SetRenderMeshes(meshes.ToArray());
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				SetRenderMeshes();
			}
		}

		public void SetRenderMeshes(params Mesh[] meshes)
		{
			Models.Children.Clear();
			bool firstMesh = true;
			BoundingBox bb = new BoundingBox();
			_centerOfMass = new Point3D();
			_centerOfGeometry = new Point3D();
			foreach (var mesh in meshes)
			{
				var bakedMesh = ResourceCache.GetModel(mesh);
				Models.Children.Add(bakedMesh.Mesh);
				_centerOfMass = Point3D.Add(_centerOfMass, bakedMesh.CenterOfMass.ToVector3D());
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
				_centerOfMass.X /= meshes.Length;
				_centerOfMass.Y /= meshes.Length;
				_centerOfMass.Z /= meshes.Length;
			}

			ClampBoundingBox(ref bb);
			_modelBoundingBox = bb;
			_centerOfGeometry = _modelBoundingBox.Center.ToPoint3D();
			ReferenceScale = CalculateReferenceScale(_modelBoundingBox);
			//UpdateGridLines(bb);

			RaisePropertyChanged("Models");
			RefocusCenter();
			RaisePropertyChanged("ReferenceScale");
		}

		protected override void RefocusCenter()
		{
			switch (CenterMode)
			{
				case 1: // mass
					CameraTarget = _centerOfMass;
					break;
				case 2: // geo
					CameraTarget = _centerOfGeometry;
					break;
				default:
					CameraTarget = new Point3D();
					break;
			}
			RaisePropertyChanged("CameraTarget");
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

		private void ClampBoundingBox(ref BoundingBox bb)
		{
			bb.Minimum.X = Math.Max(bb.Minimum.X, -MAX_GRID_LENGTH);
			bb.Minimum.Y = Math.Max(bb.Minimum.Y, -MAX_GRID_LENGTH);
			bb.Minimum.Z = Math.Max(bb.Minimum.Z, -MAX_GRID_LENGTH);
			bb.Maximum.X = Math.Min(bb.Maximum.X, MAX_GRID_LENGTH);
			bb.Maximum.Y = Math.Min(bb.Maximum.Y, MAX_GRID_LENGTH);
			bb.Maximum.Z = Math.Min(bb.Maximum.Z, MAX_GRID_LENGTH);
		}

		private double CalculateReferenceScale(BoundingBox bb)
		{
			return Math.Max(bb.Size.Length(), 0.001);
		}

		private void UpdateGridLines(BoundingBox bb)
		{
			GridWidth = bb.Width + 16;
			GridLength = bb.Height + 16;
			RaisePropertyChanged("GridWidth");
			RaisePropertyChanged("GridLength");
		}
	}
}