using OpenTK.Wpf;
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
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using TpacTool.Lib;
using static OpenTK.Graphics.OpenGL.GL;
using System.Windows.Forms;
using Binding = System.Windows.Data.Binding;
using System.ComponentModel;
using GalaSoft.MvvmLight.Messaging;
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
using System.Windows.Media.Media3D;
using System.Drawing;
using Point = System.Windows.Point;
using OpenTK.Platform;
using System.Windows.Interop;

namespace TpacTool
{
	/// <summary>
	/// OglPreviewPage.xaml 的交互逻辑
	/// </summary>
	public partial class OglPreviewPage : Page
	{
		public static Guid ChangeCursorEvent = Guid.NewGuid();

		private Renderer _renderer;

		//private List<MeshManager.OglMesh> _previewMeshes = new List<MeshManager.OglMesh>();

		//private List<TextureManager.OglTexture> _textureOfPreviewMeshes = new List<TextureManager.OglTexture>();

		private TextureManager.OglTexture _previewTexture;

		private Point _mouseDownPoint;

		private bool _isMouseDown = false;

		private bool _checkInertia = false;

		private DateTime _mouseUpTime = DateTime.Now;

		private bool _shouldUpdate;

		private bool _configChanged = false;

		private bool _firstTimeUpdateCamera = true;

		private double _oldReferenceScale;

		public enum Mode
		{
			Model,
			Image,
			Skeleton,
			Animation
		}

		#region Meshes
		public static readonly DependencyProperty MeshesProperty = DependencyProperty.Register(
			nameof(Meshes), typeof(Mesh[]),
			typeof(OglPreviewPage),
			new PropertyMetadata(OnMeshesChanged));

		private static void OnMeshesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var obj = (OglPreviewPage)d;
			obj._shouldUpdate = true;
		}

		public Mesh[] Meshes
		{
			get => (Mesh[]) GetValue(MeshesProperty);
			set => SetValue(MeshesProperty, value);
		}
		#endregion

		#region PreviewTarget
		public static readonly DependencyProperty PreviewTargetProperty = DependencyProperty.Register(
			nameof(PreviewTarget), typeof(Mode),
			typeof(OglPreviewPage),
			new PropertyMetadata(Mode.Model));

		public Mode PreviewTarget
		{
			get => (Mode) GetValue(PreviewTargetProperty);
			set => SetValue(PreviewTargetProperty, value);
		}
		#endregion

		#region ClearOnNextTick
		public static readonly DependencyProperty ClearOnNextTickProperty = DependencyProperty.Register(
			nameof(ClearOnNextTick), typeof(bool),
			typeof(OglPreviewPage),
			new PropertyMetadata(false));

		public bool ClearOnNextTick
		{
			get => (bool)GetValue(ClearOnNextTickProperty);
			set => SetValue(ClearOnNextTickProperty, value);
		}
		#endregion

		#region Texture
		public static readonly DependencyProperty TextureProperty = DependencyProperty.Register(
			nameof(Texture), typeof(Texture),
			typeof(OglPreviewPage),
			new PropertyMetadata(null, OnTextureChanged));

		private static void OnTextureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var obj = (OglPreviewPage)d;
			obj._shouldUpdate = true;
		}

		public Texture Texture
		{
			get => (Texture)GetValue(TextureProperty);
			set => SetValue(TextureProperty, value);
		}
		#endregion

		#region MaxTextureSize
		public static readonly DependencyProperty MaxTextureSizeProperty = DependencyProperty.Register(
			nameof(MaxTextureSize), typeof(int),
			typeof(OglPreviewPage),
			new PropertyMetadata(0));

		public int MaxTextureSize
		{
			get => (int)GetValue(MaxTextureSizeProperty);
			set => SetValue(MaxTextureSizeProperty, value);
		}
		#endregion

		#region ImageColorMask
		public static readonly DependencyProperty ImageColorMaskProperty = DependencyProperty.Register(
			nameof(ImageColorMask), typeof(Matrix4),
			typeof(OglPreviewPage),
			new PropertyMetadata(Matrix4.Identity));

		public Matrix4 ImageColorMask
		{
			get => (Matrix4)GetValue(ImageColorMaskProperty);
			set => SetValue(ImageColorMaskProperty, value);
		}
		#endregion

		#region EnableAlpha
		public static readonly DependencyProperty EnableAlphaProperty = DependencyProperty.Register(
			nameof(EnableAlpha), typeof(bool),
			typeof(OglPreviewPage),
			new PropertyMetadata(false));

		public bool EnableAlpha
		{
			get => (bool)GetValue(EnableAlphaProperty);
			set => SetValue(EnableAlphaProperty, value);
		}
		#endregion

		public static readonly DependencyProperty CameraTargetProperty =
			DependencyProperty.Register(
				nameof(CameraTarget), typeof(Vector3),
				typeof(OglPreviewPage),
				new UIPropertyMetadata(new Vector3(), OnCameraTargetChanged));

		private static void OnCameraTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var obj = (OglPreviewPage)d;
			obj._configChanged = true;
		}

		public Vector3 CameraTarget
		{
			get => (Vector3)GetValue(CameraTargetProperty);
			set => SetValue(CameraTargetProperty, value);
		}

		public static readonly DependencyProperty ReferenceScaleProperty =
			DependencyProperty.Register(
				nameof(ReferenceScale), typeof(double),
				typeof(OglPreviewPage),
				new UIPropertyMetadata(1d, OnReferenceScaleChanged));

		private static void OnReferenceScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var obj = (OglPreviewPage)d;
			obj._configChanged = true;
		}

		public double ReferenceScale
		{
			get => (double)GetValue(ReferenceScaleProperty);
			set => SetValue(ReferenceScaleProperty, value);
		}

		public static readonly DependencyProperty KeepScaleModeProperty =
			DependencyProperty.Register(
				nameof(KeepScaleMode), typeof(bool),
				typeof(OglPreviewPage),
				new PropertyMetadata(true, OnKeepScaleModeChanged)
			);

		private static void OnKeepScaleModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var obj = (OglPreviewPage)d;
			obj._configChanged = true;
		}

		public bool KeepScaleMode
		{
			get => (bool)GetValue(KeepScaleModeProperty);
			set => SetValue(KeepScaleModeProperty, value);
		}

		public static readonly DependencyProperty EnableInertiaProperty =
			DependencyProperty.Register(
				nameof(EnableInertia), typeof(bool),
				typeof(OglPreviewPage),
				new PropertyMetadata(true, OnEnableInertiaChanged)
			);

		private static void OnEnableInertiaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var obj = (OglPreviewPage)d;
			obj._configChanged = true;
		}

		public bool EnableInertia
		{
			get => (bool)GetValue(EnableInertiaProperty);
			set => SetValue(EnableInertiaProperty, value);
		}

		public static readonly DependencyProperty EnableTransitionInertiaProperty =
			DependencyProperty.Register(
				nameof(EnableTransitionInertia), typeof(bool),
				typeof(OglPreviewPage),
				new PropertyMetadata(true, OnEnableTransitionInertiaChanged)
			);

		private static void OnEnableTransitionInertiaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var obj = (OglPreviewPage)d;
			obj._configChanged = true;
		}

		public bool EnableTransitionInertia
		{
			get => (bool)GetValue(EnableTransitionInertiaProperty);
			set => SetValue(EnableTransitionInertiaProperty, value);
		}

		public static readonly DependencyProperty EnableScaleInertiaProperty =
			DependencyProperty.Register(
				nameof(EnableScaleInertia), typeof(bool),
				typeof(OglPreviewPage),
				new PropertyMetadata(true, OnEnableScaleInertiaChanged)
			);

		private static void OnEnableScaleInertiaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var obj = (OglPreviewPage)d;
			obj._configChanged = true;
		}

		public bool EnableScaleInertia
		{
			get => (bool)GetValue(EnableScaleInertiaProperty);
			set => SetValue(EnableScaleInertiaProperty, value);
		}

		public static readonly DependencyProperty ShowGridLinesProperty =
			DependencyProperty.Register(
				nameof(ShowGridLines), typeof(bool),
				typeof(OglPreviewPage),
				new PropertyMetadata(true, OnShowGridLinesChanged)
			);

		private static void OnShowGridLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var obj = (OglPreviewPage)d;
			obj._configChanged = true;
		}

		public bool ShowGridLines
		{
			get => (bool)GetValue(ShowGridLinesProperty);
			set => SetValue(ShowGridLinesProperty, value);
		}

		public static readonly DependencyProperty GridLineXProperty =
			DependencyProperty.Register(
				nameof(GridLineX), typeof(int),
				typeof(OglPreviewPage),
				new PropertyMetadata(OnGridLineXChanged)
			);

		private static void OnGridLineXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var obj = (OglPreviewPage)d;
			obj._configChanged = true;
		}

		public int GridLineX
		{
			get => (int)GetValue(GridLineXProperty);
			set => SetValue(GridLineXProperty, value);
		}

		public static readonly DependencyProperty GridLineYProperty =
			DependencyProperty.Register(
				nameof(GridLineY), typeof(int),
				typeof(OglPreviewPage),
				new PropertyMetadata(OnGridLineYChanged)
			);

		private static void OnGridLineYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var obj = (OglPreviewPage)d;
			obj._configChanged = true;
		}

		public int GridLineY
		{
			get => (int)GetValue(GridLineYProperty);
			set => SetValue(GridLineYProperty, value);
		}

		public static readonly DependencyProperty LightModeProperty =
			DependencyProperty.Register(
				nameof(LightMode), typeof(int),
				typeof(OglPreviewPage),
				new PropertyMetadata(OnLightModeChanged)
			);

		private static void OnLightModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var obj = (OglPreviewPage)d;
			obj._configChanged = true;
		}

		public int LightMode
		{
			get => (int)GetValue(LightModeProperty);
			set => SetValue(LightModeProperty, value);
		}

		public static readonly DependencyProperty ModelBoundingBoxProperty =
			DependencyProperty.Register(
				nameof(ModelBoundingBox), typeof(BoundingBox),
				typeof(OglPreviewPage),
				new PropertyMetadata(OnModelBoundingBoxChanged)
			);

		private static void OnModelBoundingBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var obj = (OglPreviewPage)d;
			obj._configChanged = true;
		}

		public BoundingBox ModelBoundingBox
		{
			get => (BoundingBox)GetValue(ModelBoundingBoxProperty);
			set => SetValue(ModelBoundingBoxProperty, value);
		}

		public OglPreviewPage()
		{
			InitializeComponent();

			Messenger.Default.Register<Point>(this, MainWindow.MouseMoveUserEvent, OnMouseMove);
			Messenger.Default.Register<(MouseButton, Point)>(this, MainWindow.MouseUpUserEvent, OnMouseUp);

			SetBinding(MeshesProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(Meshes)),
				Mode = BindingMode.OneWay
			});

			SetBinding(PreviewTargetProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(PreviewTarget)),
				Mode = BindingMode.OneWay
			});

			SetBinding(ClearOnNextTickProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(ClearOnNextTick)),
				Mode = BindingMode.TwoWay
			});

			SetBinding(TextureProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(Texture)),
				Mode = BindingMode.OneWay
			});

			SetBinding(MaxTextureSizeProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(MaxTextureSize)),
				Mode = BindingMode.OneWay
			});

			SetBinding(ImageColorMaskProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(ImageColorMask)),
				Mode = BindingMode.OneWay
			});

			SetBinding(EnableAlphaProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(EnableAlpha)),
				Mode = BindingMode.OneWay
			});

			SetBinding(CameraTargetProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(CameraTarget)),
				Mode = BindingMode.OneWay
			});

			SetBinding(ReferenceScaleProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(ReferenceScale)),
				Mode = BindingMode.OneWay
			});

			SetBinding(KeepScaleModeProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(KeepScaleMode)),
				Mode = BindingMode.OneWay
			});

			SetBinding(EnableInertiaProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(EnableInertia)),
				Mode = BindingMode.OneWay
			});

			SetBinding(EnableTransitionInertiaProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(EnableTransitionInertia)),
				Mode = BindingMode.OneWay
			});

			SetBinding(EnableScaleInertiaProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(EnableScaleInertia)),
				Mode = BindingMode.OneWay
			});

			SetBinding(ShowGridLinesProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(ShowGridLines)),
				Mode = BindingMode.OneWay
			});

			SetBinding(GridLineXProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(GridLineX)),
				Mode = BindingMode.OneWay
			});

			SetBinding(GridLineYProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(GridLineY)),
				Mode = BindingMode.OneWay
			});

			SetBinding(LightModeProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(LightMode)),
				Mode = BindingMode.OneWay
			});

			SetBinding(ModelBoundingBoxProperty, new Binding()
			{
				Source = DataContext,
				Path = new PropertyPath(nameof(ModelBoundingBox)),
				Mode = BindingMode.OneWay
			});

			var settings = new GLWpfControlSettings
			{
				MajorVersion = 4,
				MinorVersion = 2,
				RenderContinuously = true,
				GraphicsProfile = OpenTK.Graphics.OpenGL.ContextProfileMask.ContextCoreProfileBit,
				GraphicsContextFlags = GraphicsContextFlags.ForwardCompatible,
			};

			OpenTkControl.Start(settings);
		}

		private void OpenTkControl_Ready()
		{
			if (_renderer == null)
			{
				_renderer = new Renderer(OpenTkControl);
				_renderer.Ready();
			}
		}

		private void OpenTkControl_Render(TimeSpan delta)
		{
			if (ClearOnNextTick)
			{
				ClearOnNextTick = false;
				_shouldUpdate = true;

				MeshManager.Clear();
				TextureManager.Clear();
			}
			if (PreviewTarget == Mode.Image)
			{
				if (_shouldUpdate)
				{
					var newTex = Texture;
					_previewTexture = newTex != null ? TextureManager.Get(newTex, MaxTextureSize) : null;
					_shouldUpdate = false;
				}

				_renderer.ImageColorMask = ImageColorMask;
				_renderer.EnableAlpha = EnableAlpha;
				_renderer.RenderImage(_previewTexture);
			}
			else
			{
				if (_configChanged)
				{
					_configChanged = false;
					var point = CameraTarget;
					if (_firstTimeUpdateCamera)
					{
						_firstTimeUpdateCamera = false;
						_renderer.InitCamera((float)point.X, (float)point.Y, (float)point.Z, 45f, 25f, 5);
					}
					else
					{
						_renderer.SetCameraTarget((float)point.X, (float)point.Y, (float)point.Z);
						if (KeepScaleMode)
						{
							var distance = _renderer.CameraDistanceTarget;
							distance = (float)(distance / _oldReferenceScale * ReferenceScale);
							if (EnableScaleInertia)
								_renderer.CameraDistanceTarget = distance;
							else
								_renderer.CameraDistance = distance;
						}
					}
					_oldReferenceScale = ReferenceScale;
					_renderer.EnableInertia = EnableInertia;
					_renderer.EnableTransitionInertia = EnableTransitionInertia;
					_renderer.EnableScaleInertia = EnableScaleInertia;
					_renderer.ShowGrid = ShowGridLines;
					_renderer.GridX = GridLineX;
					_renderer.GridY = GridLineY;
					_renderer.SetLight(LightMode, ModelBoundingBox);
				}

				if (_shouldUpdate)
				{
					_renderer.Meshes.Clear();
					if (Meshes != null)
					{
						foreach (var mesh in Meshes)
						{
							var rm = new Renderer.RenderMesh();
							rm.Mesh = MeshManager.Get(mesh);
							if (mesh.Material.TryGetItem(out var mat)
							    && mat != null
							    && mat.Textures.TryGetValue(0, out var texRef)
							    && texRef.TryGetItem(out var tex))
							{
								rm.Texture = TextureManager.Get(tex, 2048);
								rm.AlphaTestValue = mat.AlphaTest;
								if (mat.Flags.Contains("two_sided"))
									rm.DoubleSides = true;
								if (mat.ShaderMaterialFlags.Contains("alpha_test"))
									rm.AlphaTest = true;
							}
							else
							{
								rm.Texture = TextureManager.FALLBACK_TEXTURE;
							}
							rm.Shader = ShaderManager.MeshShader;
							_renderer.Meshes.Add(rm);
						}
					}
					_shouldUpdate = false;
				}

				_renderer.Render(delta);
			}
		}

		private void OpenTkControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			_renderer.Resize();
		}

		private void OpenTkControl_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Right && !_isMouseDown)
			{
				_isMouseDown = true;
				_renderer.ClearMouseMoveInertia();
				Messenger.Default.Send<Cursor>(Cursors.SizeAll, ChangeCursorEvent);
			}
		}

		private void OpenTkControl_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			_renderer.ApplyMouseWheel(e.Delta);
		}

		private void OnMouseMove(Point point)
		{
			if (_isMouseDown)
			{
				_renderer.ApplyMouseMove(point.X - _mouseDownPoint.X, point.Y - _mouseDownPoint.Y);
			}
			else if (_checkInertia)
			{
				_checkInertia = false;
				var x = point.X - _mouseDownPoint.X;
				var y = point.Y - _mouseDownPoint.Y;
				var timeSpan = DateTime.Now - _mouseUpTime;
				var time = timeSpan.TotalSeconds;
				if (time < 0.1)
				{
					time = 1 - time * 10;
					_renderer.AddMouseMoveInertia(x * time, y * time);
				}
			}
			_mouseDownPoint = point;
		}

		private void OnMouseUp((MouseButton, Point) args)
		{
			if (args.Item1 == MouseButton.Right && _isMouseDown)
			{
				_isMouseDown = false;
				_checkInertia = true;
				_mouseUpTime = DateTime.Now;
				Messenger.Default.Send<Cursor>(null, ChangeCursorEvent);
			}
		}
	}
}
