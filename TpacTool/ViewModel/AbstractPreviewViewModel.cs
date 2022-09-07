using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using GalaSoft.MvvmLight;
using TpacTool.Lib;
using TpacTool.Properties;

namespace TpacTool
{
	public abstract class AbstractPreviewViewModel : ViewModelBase
	{
		public static readonly Guid PreviewAssetEvent = Guid.NewGuid();

		public static bool SupportDx { set; get; }

		public static Uri ModelPreviewUri { set; get; }

		protected const float MAX_GRID_LENGTH = 256;

		internal static string LIGHTMODE_SUN = Resources.Preview_Lights_Single;
		internal static string LIGHTMODE_THREEPOINTS = Resources.Preview_Lights_Tri;
		internal static string LIGHTMODE_DEFAULT = Resources.Preview_Lights_Quad;

		internal static string CENTERMODE_ORIGIN = Resources.Preview_Center_Origin;
		//internal static string CENTERMODE_BOUNDINGBOX = "Center of Bounding Box";
		internal static string CENTERMODE_MASS = Resources.Preview_Center_Mass;
		internal static string CENTERMODE_CENTER = Resources.Preview_Center_Geometry;

		internal static string KEEPSCALEMODE_OFF = "Off";
		internal static string KEEPSCALEMODE_ON = "On";
		internal static string KEEPSCALEMODE_ON_INERTIAL = "On, inertial";

		internal static string[] _lightModeItems = new[]
		{
			LIGHTMODE_SUN,
			LIGHTMODE_THREEPOINTS,
			LIGHTMODE_DEFAULT
		};

		internal static string[] _centerModeItems = new[]
		{
			CENTERMODE_ORIGIN,
			//CENTERMODE_BOUNDINGBOX,
			CENTERMODE_MASS,
			CENTERMODE_CENTER
		};

		internal static string[] _keepScaleModeItems = new[]
		{
			KEEPSCALEMODE_OFF,
			KEEPSCALEMODE_ON,
			//KEEPSCALEMODE_ON_INERTIAL
		};

		private int _lightMode = Settings.Default.PreviewLight;
		private int _centerMode = Settings.Default.PreviewCenter;
		private bool _keepScaleMode = Settings.Default.PreviewKeepScale;
		private bool _enableInertia = Settings.Default.PreviewInertia;
		private bool _showGridLines = Settings.Default.PreviewShowGrid;

		public abstract Uri PageUri { get; }

		public virtual string[] LightModeItems => _lightModeItems;

		public virtual string[] CenterModeItems => _centerModeItems;

		public virtual string[] KeepScaleModeItems => _keepScaleModeItems;

		public BoundingBox ModelBoundingBox { get; set; } = new BoundingBox();

		protected Point3D CenterOfMass { get; set; } = new Point3D();

		protected Point3D CenterOfGeometry { get; set; } = new Point3D();

		public Point3D CameraTarget { protected set; get; } = new Point3D(0, 0, 0);

		public double ReferenceScale { protected set; get; } = 1;

		public int LightMode
		{
			set
			{
				_lightMode = value;
				UpdateLights();
				Settings.Default.PreviewLight = _lightMode;
			}
			get => _lightMode;
		}

		public int CenterMode
		{
			set
			{
				_centerMode = value;
				RefocusCenter();
				Settings.Default.PreviewCenter = _centerMode;
			}
			get => _centerMode;
		}

		public bool KeepScaleMode
		{
			set
			{
				_keepScaleMode = value;
				RaisePropertyChanged(nameof(KeepScaleMode));
				Settings.Default.PreviewKeepScale = _keepScaleMode;
			}
			get => _keepScaleMode;
		}

		public bool EnableInertia
		{
			set
			{
				_enableInertia = value;
				RaisePropertyChanged(nameof(EnableInertia));
				Settings.Default.PreviewInertia = _enableInertia;
			}
			get => _enableInertia;
		}

		public bool ShowGridLines
		{
			set
			{
				_showGridLines = value;
				RaisePropertyChanged(nameof(ShowGridLines));
				Settings.Default.PreviewShowGrid = _showGridLines;
			}
			get => _showGridLines;
		}

		public double GridWidth { set; get; } = 40;

		public double GridLength { set; get; } = 40;

		protected AbstractPreviewViewModel()
		{
			if (!IsInDesignMode)
			{
				MessengerInstance.Register<List<Mesh>>(this, PreviewAssetEvent, OnPreviewModel);
			}
		}

		protected virtual void OnPreviewModel(List<Mesh> meshes)
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

		public virtual void SetRenderMetamesh(Metamesh metamesh, int lod = 0)
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

		public abstract void SetRenderMeshes(params Mesh[] meshes);

		protected virtual void RefocusCenter()
		{
			switch (CenterMode)
			{
				case 1: // mass
					CameraTarget = CenterOfMass;
					break;
				case 2: // geo
					CameraTarget = CenterOfGeometry;
					break;
				default:
					CameraTarget = new Point3D();
					break;
			}
			RaisePropertyChanged(nameof(CameraTarget));
		}

		protected abstract void UpdateLights();

		protected void ClampBoundingBox(ref BoundingBox bb)
		{
			bb.Min = Vector3.Max(bb.Min, new Vector3(-MAX_GRID_LENGTH));
			bb.Max = Vector3.Min(bb.Max, new Vector3(MAX_GRID_LENGTH));
		}

		protected double CalculateReferenceScale(BoundingBox bb)
		{
			return Math.Max(bb.BoundingSphereRadius, 0.001);
		}
	}
}