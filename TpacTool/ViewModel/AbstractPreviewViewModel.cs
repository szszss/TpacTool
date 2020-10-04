using System;
using System.Collections.Generic;
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

		protected const float MAX_GRID_LENGTH = 256;

		internal static string LIGHTMODE_SUN = "Single Light";
		internal static string LIGHTMODE_THREEPOINTS = "Tri Lights";
		internal static string LIGHTMODE_DEFAULT = "Quad Lights";

		internal static string CENTERMODE_ORIGIN = "Origin";
		//internal static string CENTERMODE_BOUNDINGBOX = "Center of Bounding Box";
		internal static string CENTERMODE_MASS = "Center of Mass";
		internal static string CENTERMODE_CENTER = "Center of Geometry";

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
				RaisePropertyChanged("KeepScaleMode");
				Settings.Default.PreviewKeepScale = _keepScaleMode;
			}
			get => _keepScaleMode;
		}

		public bool EnableInertia
		{
			set
			{
				_enableInertia = value;
				RaisePropertyChanged("EnableInertia");
				Settings.Default.PreviewInertia = _enableInertia;
			}
			get => _enableInertia;
		}

		public bool ShowGridLines
		{
			set
			{
				_showGridLines = value;
				RaisePropertyChanged("ShowGridLines");
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

		protected abstract void OnPreviewModel(List<Mesh> meshes);

		protected abstract void RefocusCenter();

		protected abstract void UpdateLights();
	}
}