using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Media;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using OpenTK;
using TpacTool.Lib;
using TpacTool.Properties;

namespace TpacTool
{
	public sealed class OglPreviewViewModel : AbstractPreviewViewModel
	{
		public static readonly Guid PreviewTextureEvent = Guid.NewGuid();

		private static Mesh[] emptyMeshes = new Mesh[0];

		private bool _enableTransitionInertia = Settings.Default.PreviewTransitionInertia;

		private bool _enableScaleInertia = Settings.Default.PreviewScaleInertia;

		public override Uri PageUri { get; } = new Uri("../Page/OglPreviewPage.xaml", UriKind.Relative);

		public OglPreviewPage.Mode PreviewTarget { private set; get; } = OglPreviewPage.Mode.Model;

		public bool IsModelMode => PreviewTarget == OglPreviewPage.Mode.Model;

		public bool IsImageMode => PreviewTarget == OglPreviewPage.Mode.Image;

		public bool EnableTransitionInertia
		{
			set
			{
				_enableTransitionInertia = value;
				RaisePropertyChanged(nameof(EnableTransitionInertia));
				Settings.Default.PreviewTransitionInertia = _enableTransitionInertia;
			}
			get => _enableTransitionInertia;
		}

		public bool EnableScaleInertia
		{
			set
			{
				_enableScaleInertia = value;
				RaisePropertyChanged(nameof(EnableScaleInertia));
				Settings.Default.PreviewScaleInertia = _enableScaleInertia;
			}
			get => _enableScaleInertia;
		}

		public bool ClearOnNextTick { set; get; }

		public int GridLineX { set; get; }

		public int GridLineY { set; get; }

		#region Model

		public Mesh[] Meshes { private set; get; } = emptyMeshes;

		#endregion

		#region Texture & Material

		public string ImageText { private set; get; }

		public Texture Texture { set; get; }

		public int MaxTextureSize { set; get; }

		public Matrix4 ImageColorMask { set; get; } = Matrix4.Identity;

		public bool EnableAlpha { set; get; } = false;

		#endregion

		public OglPreviewViewModel()
		{
			if (!IsInDesignMode)
			{
				UpdateLights();
				MessengerInstance.Register<Texture>(this, PreviewTextureEvent, SetRenderTexture);
				MessengerInstance.Register<object>(this, MainViewModel.CleanupEvent, OnCleanup);
			}
		}

		private void OnCleanup(object unused = null)
		{
			//Models.Children.Clear();
			//RaisePropertyChanged("Models");
			ImageText = string.Empty;
			Texture = null;
			RaisePropertyChanged(nameof(ImageText));
			RaisePropertyChanged(nameof(Texture));
		}

		public override void SetRenderMeshes(params Mesh[] meshes)
		{
			SetPreviewTarget(OglPreviewPage.Mode.Model);
			Meshes = meshes;

			bool firstMesh = true;
			BoundingBox bb = new BoundingBox();
			CenterOfMass = new Point3D();
			CenterOfGeometry = new Point3D();

			foreach (var mesh in meshes)
			{
				double comX = 0d, comY = 0d, comZ = 0d;
				if (mesh.VertexStream != null)
				{
					foreach (var position in mesh.VertexStream.Data.Positions)
					{
						comX += position.X;
						comY += position.Y;
						comZ += position.Z;
					}
					var vertexCount = mesh.VertexStream.Data.Positions.Length;
					if (vertexCount > 0)
					{
						comX /= vertexCount;
						comY /= vertexCount;
						comZ /= vertexCount;
					}
				}
				else if (mesh.EditData != null)
				{
					foreach (var position in mesh.EditData.Data.Positions)
					{
						comX += position.X;
						comY += position.Y;
						comZ += position.Z;
					}
					var vertexCount = mesh.EditData.Data.Positions.Length;
					if (vertexCount > 0)
					{
						comX /= vertexCount;
						comY /= vertexCount;
						comZ /= vertexCount;
					}
				}

				CenterOfMass = Point3D.Add(CenterOfMass, new Vector3D(comX, comY, comZ));
				if (firstMesh)
				{
					bb = mesh.BoundingBox;
					firstMesh = false;
				}
				else
				{
					bb = BoundingBox.Merge(bb, mesh.BoundingBox);
				}
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
			GridLineX = (int) bb.Width + 16;
			GridLineY = (int) bb.Depth + 16;
			RaisePropertyChanged(nameof(GridLineX));
			RaisePropertyChanged(nameof(GridLineY));

			RaisePropertyChanged(nameof(Meshes));
			RefocusCenter();
			RaisePropertyChanged(nameof(ReferenceScale));
			RaisePropertyChanged(nameof(ModelBoundingBox));
		}

		protected override void UpdateLights()
		{
			RaisePropertyChanged(nameof(LightMode));
		}

		public void SetRenderTexture(Texture texture)
		{
			SetPreviewTarget(OglPreviewPage.Mode.Image);
			Texture = null;
			if (texture == null)
			{
				ImageText = string.Empty;
			}
			else if (texture.HasPixelData && texture.TexturePixels.IsLargeData)
			{
				ImageText = Resources.Preview_Msg_TextureSizeTooLarge;
			}
			else if (!texture.HasPixelData)
			{
				ImageText = Resources.Preview_Msg_TextureHasNoData;
			}
			else if (!TextureManager.IsFormatSupported(texture.Format))
			{
				ImageText = string.Format(Resources.Preview_Msg_TextureFormatUnsupported, texture.Format.ToString());
			}
			else
			{
				ImageText = string.Empty;
				Texture = texture;
				var maxTextureSize = TextureViewModel._clampMode;
				var textureChannelMode = TextureViewModel._channelMode;
				switch (maxTextureSize)
				{
					case 1: // 2048
						maxTextureSize = 2048; break;
					case 2: // 1024
						maxTextureSize = 1024; break;
					case 3: // 512
						maxTextureSize = 512; break;
				}

				if (maxTextureSize != MaxTextureSize)
				{
					MaxTextureSize = maxTextureSize;
					ClearOnNextTick = true;
					RaisePropertyChanged(nameof(MaxTextureSize));
					RaisePropertyChanged(nameof(ClearOnNextTick));
				}

				if (textureChannelMode == 0)
				{
					switch (texture.Format.GetColorChannel())
					{
						case 4:
							textureChannelMode = ResourceCache.CHANNEL_MODE_RGBA; break;
						case 3:
							textureChannelMode = ResourceCache.CHANNEL_MODE_RGB; break;
						case 2:
							textureChannelMode = ResourceCache.CHANNEL_MODE_RG; break;
						case 1:
							textureChannelMode = ResourceCache.CHANNEL_MODE_R; break;
					}
				}

				EnableAlpha = false;
				switch (textureChannelMode)
				{
					case ResourceCache.CHANNEL_MODE_RGBA:
						ImageColorMask = Matrix4.Identity;
						EnableAlpha = true;
						break;
					case ResourceCache.CHANNEL_MODE_RGB:
						ImageColorMask = Matrix4.Identity;
						break;
					case ResourceCache.CHANNEL_MODE_RG:
						ImageColorMask = new Matrix4(
							1, 0, 0, 0,
							0, 1, 0, 0,
							0, 0, 0, 0,
							0, 0, 0, 0
							);
						break;
					case ResourceCache.CHANNEL_MODE_R:
						ImageColorMask = new Matrix4(
							1, 1, 1, 0,
							0, 0, 0, 0,
							0, 0, 0, 0,
							0, 0, 0, 0
						);
						break;
					case ResourceCache.CHANNEL_MODE_G:
						ImageColorMask = new Matrix4(
							0, 0, 0, 0,
							1, 1, 1, 0,
							0, 0, 0, 0,
							0, 0, 0, 0
						);
						break;
					case ResourceCache.CHANNEL_MODE_B:
						ImageColorMask = new Matrix4(
							0, 0, 0, 0,
							0, 0, 0, 0,
							1, 1, 1, 0,
							0, 0, 0, 0
						);
						break;
					case ResourceCache.CHANNEL_MODE_ALPHA:
						ImageColorMask = new Matrix4(
							0, 0, 0, 0,
							0, 0, 0, 0,
							0, 0, 0, 0,
							1, 1, 1, 0
						);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(textureChannelMode));
				}

				RaisePropertyChanged(nameof(ImageColorMask));
				RaisePropertyChanged(nameof(EnableAlpha));
			}
			RaisePropertyChanged(nameof(ImageText));
			RaisePropertyChanged(nameof(Texture));
		}

		private void SetPreviewTarget(OglPreviewPage.Mode mode)
		{
			if (PreviewTarget != mode)
			{
				PreviewTarget = mode;
				RaisePropertyChanged(nameof(PreviewTarget));
				RaisePropertyChanged(nameof(IsModelMode));
				RaisePropertyChanged(nameof(IsImageMode));
			}
		}
	}
}
