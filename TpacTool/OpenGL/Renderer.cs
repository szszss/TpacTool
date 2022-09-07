using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Wpf;
using OpenTK.Graphics.OpenGL4;
using TpacTool.Lib;
using static OpenTK.Graphics.OpenGL4.GL;
using RenderbufferStorage = OpenTK.Graphics.OpenGL4.RenderbufferStorage;
using System.Windows.Controls;

namespace TpacTool
{
	public class Renderer
	{
		private readonly GLWpfControl _control;
		private readonly MeshManager.OglMesh _imageMesh;
		private readonly MeshManager.OglMesh _gridMesh;
		private readonly MeshManager.OglMesh _arrowMesh;

		public const float GRID_LINEWIDTH = 0.005f;

		public const float CAM_DISTANCE_MIN = 1;
		public const float CAM_DISTANCE_MAX = 50;

		public const float CAM_DISTANCE_INERTIA_FACTOR = 7f;
		public const float CAM_MOUSEMOVE_INERTIA_FACTOR = 3f;
		public const float CAM_AIM_INERTIA_FACTOR = 6f;

		public const float CAM_MOUSEWHEEL_FACTOR_ON_MIN = 0.004f;
		public const float CAM_MOUSEWHEEL_FACTOR_ON_MAX = 0.05f;

		public const float CAM_MOUSEMOVE_FACTOR = 0.5f;
		public const float CAM_MOUSEMOVE_INERTIA_ADD_FACTOR = 30f;
		public const float CAM_MOUSEMOVE_INERTIA_THRESHOLD = 2;

		public const int MSAA_SAMPLES = 4;
		
		private Matrix4 _projectionMatrix;
		private int _controlWidth, _controlHeight;
		private double _cameraYaw;
		private double _cameraPitch;
		private float _cameraDistance = 5;

		private double _cameraYawInertiaTarget;
		private double _cameraPitchInertiaTarget;
		private float _cameraDistanceInertiaTarget = 5;

		private int _uboLights;

		private ShaderManager.LightBlock _lightBlock;

		public int GridX { set; get; } = 4;

		public int GridY { set; get; } = 4;

		private int _framebufferId = -1;

		private int _framebufferColorTexId = -1;

		private int _framebufferDepthTexId = -1;

		public Vector3 _cameraAim;

		public Vector3 _cameraAimInertiaTarget;

		public Matrix4 ImageColorMask { set; get; } = Matrix4.Identity;

		public bool EnableAlpha { set; get; } = false;

		public bool EnableFaceCull { set; get; } = true;

		public bool EnableInertia { set; get; } = true;

		public bool EnableTransitionInertia { set; get; } = true;

		public bool EnableScaleInertia { set; get; } = true;

		public bool ShowGrid { set; get; } = true;

		public float CameraDistance
		{
			set
			{
				CameraDistanceTarget = value;
				_cameraDistance = CameraDistanceTarget;
			}
			get => _cameraDistance;
		}

		public float CameraDistanceTarget
		{
			set => _cameraDistanceInertiaTarget = MathHelper.Clamp(value, CAM_DISTANCE_MIN, CAM_DISTANCE_MAX);
			get => _cameraDistanceInertiaTarget;
		}

		/*public double CameraYaw
		{
			set
			{
				_cameraYawInertiaTarget = value;
			}
			get => _cameraYawInertiaTarget;
		}

		public double CameraPitch
		{
			set
			{
				_cameraPitchInertiaTarget = value;
			}
			get => _cameraPitchInertiaTarget;
		}*/

		public List<RenderMesh> Meshes { get; } = new List<RenderMesh>();

		public unsafe Renderer(GLWpfControl control)
		{
			_control = control;
			ShaderManager.Init();

			_imageMesh = new MeshManager.OglMesh(
				new[]
				{
					0, 3, 1,
					3, 2, 1
				},
				new[]
				{
					-1f, -1, 0,
					-1, 1, 0,
					1, 1, 0,
					1, -1, 0
				},
				null,
				new[]
				{
					0f, 1,
					0, 0,
					1, 0,
					1, 1
				});

			_gridMesh = new MeshManager.OglMesh(
				new[]
				{
					0, 1, 2,
					2, 3, 0
				},
				new[]
				{
					-GRID_LINEWIDTH, 0, -1,
					GRID_LINEWIDTH, 0, -1,
					GRID_LINEWIDTH, 0, 1,
					-GRID_LINEWIDTH, 0, 1
				},
				new[]
				{
					0f, 1, 0,
					0, 1, 0,
					0, 1, 0,
					0, 1, 0
				},
				new []
				{
					0f, -1f,
					1f, -1f,
					1f, 1f,
					0f, 1f
				});

			for (var i = 0; i < ARROW_MESH_INDEX.Length; i++)
			{
				ARROW_MESH_INDEX[i] -= 1;
			}
			_arrowMesh = new MeshManager.OglMesh(ARROW_MESH_INDEX, ARROW_MESH_POS);

			_uboLights = GenBuffer();
			BindBuffer(BufferTarget.UniformBuffer, _uboLights);
			BufferData(BufferTarget.UniformBuffer, sizeof(ShaderManager.LightBlock), IntPtr.Zero, BufferUsageHint.DynamicDraw);
			BindBuffer(BufferTarget.UniformBuffer, 0);
		}

		public void Ready()
		{
			RecreateFrameBuffer();
			ClearColor(0.5f, 0.5f, 0.5f, 1f);
			ClearDepth(1);
			Enable(EnableCap.DepthTest);
			Enable(EnableCap.Multisample);
		}

		public void Resize()
		{
			RecreateFrameBuffer();
		}

		public void ApplyMouseMove(double x, double y)
		{
			_cameraYawInertiaTarget += (float)x * CAM_MOUSEMOVE_FACTOR;
			_cameraPitchInertiaTarget += (float)y * CAM_MOUSEMOVE_FACTOR;

			_cameraYaw = _cameraYawInertiaTarget;
			_cameraPitch = _cameraPitchInertiaTarget;
		}

		public void ApplyMouseWheel(int delta)
		{
			var factor = (CameraDistanceTarget - CAM_DISTANCE_MIN) / (CAM_DISTANCE_MAX - CAM_DISTANCE_MIN);
			factor = CAM_MOUSEWHEEL_FACTOR_ON_MIN * (1 - factor) + CAM_MOUSEWHEEL_FACTOR_ON_MAX * factor;
			CameraDistanceTarget -= delta * factor;
		}

		public void AddMouseMoveInertia(double x, double y)
		{
			if (EnableInertia && (x > CAM_MOUSEMOVE_INERTIA_THRESHOLD || x < -CAM_MOUSEMOVE_INERTIA_THRESHOLD
			    || y > CAM_MOUSEMOVE_INERTIA_THRESHOLD || y < -CAM_MOUSEMOVE_INERTIA_THRESHOLD))
			{
				_cameraYawInertiaTarget += (float)x * CAM_MOUSEMOVE_INERTIA_ADD_FACTOR;
				_cameraPitchInertiaTarget += (float)y * CAM_MOUSEMOVE_INERTIA_ADD_FACTOR;
			}
		}

		public void ClearMouseMoveInertia()
		{
			_cameraYawInertiaTarget = _cameraYaw;
			_cameraPitchInertiaTarget = _cameraPitch;
		}

		public void SetCameraTarget(float x, float y, float z)
		{
			_cameraAimInertiaTarget = new Vector3(x, y, z);
		}

		public void InitCamera(float aimX, float aimY, float aimZ, float yaw, float pitch, float distance)
		{
			_cameraAim = _cameraAimInertiaTarget = new Vector3(aimX, aimY, aimZ);
			_cameraYaw = _cameraYawInertiaTarget = yaw;
			_cameraPitch = _cameraPitchInertiaTarget = pitch;
			_cameraDistance = _cameraDistanceInertiaTarget = distance;
		}

		public unsafe void SetLight(int mode, BoundingBox bb)
		{
			var color1 = new Vector3(1, 1, 1);
			var color2 = new Vector3(0.5f, 0.5f, 0.6f);
			var color3 = new Vector3(0.5f, 0.5f, 0.6f);
			var directionalLight = new Vector3(-10, -10, -5).Normalized();
			var center = bb == null
				? System.Numerics.Vector3.Zero
				: new System.Numerics.Vector3(-bb.Center.X, bb.Center.Z, bb.Center.Y);
			var radius = bb?.BoundingSphereRadius ?? 1f;
			var pos2 = center + radius * new System.Numerics.Vector3(-1, 1, 0);
			var pos3 = center + radius * new System.Numerics.Vector3(0, 0.5f, -1);
			switch (mode)
			{
				default: // single light
					_lightBlock = new ShaderManager.LightBlock()
					{
						Ambient = new Vector4(0.6f, 0.6f, 0.6f, 1)
					};

					_lightBlock.Enabled[0] = 1;
					_lightBlock.Color[0] = color1.X;
					_lightBlock.Color[1] = color1.Y;
					_lightBlock.Color[2] = color1.Z;
					_lightBlock.Position[0] = directionalLight.X;
					_lightBlock.Position[1] = directionalLight.Y;
					_lightBlock.Position[2] = directionalLight.Z;
					_lightBlock.IsDirectional[0] = 1;
					break;
				case 1:
					_lightBlock = new ShaderManager.LightBlock()
					{
						Ambient = new Vector4(0.3f, 0.3f, 0.3f, 1)
					};

					color1 = new Vector3(0.8f, 0.8f, 0.8f);
					_lightBlock.Enabled[0] = 1;
					_lightBlock.Color[0] = color1.X;
					_lightBlock.Color[1] = color1.Y;
					_lightBlock.Color[2] = color1.Z;
					_lightBlock.Position[0] = directionalLight.X;
					_lightBlock.Position[1] = directionalLight.Y;
					_lightBlock.Position[2] = directionalLight.Z;
					_lightBlock.IsDirectional[0] = 1;

					_lightBlock.Enabled[1] = 1;
					_lightBlock.Color[4] = color2.X;
					_lightBlock.Color[5] = color2.Y;
					_lightBlock.Color[6] = color2.Z;
					_lightBlock.Position[4] = pos2.X;
					_lightBlock.Position[5] = pos2.Y;
					_lightBlock.Position[6] = pos2.Z;

					_lightBlock.Enabled[2] = 1;
					_lightBlock.Color[8] = color3.X;
					_lightBlock.Color[9] = color3.Y;
					_lightBlock.Color[10] = color3.Z;
					_lightBlock.Position[8] = pos3.X;
					_lightBlock.Position[9] = pos3.Y;
					_lightBlock.Position[10] = pos3.Z;
					break;
				case 2:
					_lightBlock = new ShaderManager.LightBlock()
					{
						Ambient = new Vector4(0.3f, 0.3f, 0.3f, 1)
					};

					directionalLight = new Vector3(0, -1, -1).Normalized();
					color1 = new Vector3(0.4f, 0.4f, 0.4f);
					_lightBlock.Enabled[0] = 1;
					_lightBlock.Color[0] = color1.X;
					_lightBlock.Color[1] = color1.Y;
					_lightBlock.Color[2] = color1.Z;
					_lightBlock.Position[0] = directionalLight.X;
					_lightBlock.Position[1] = directionalLight.Y;
					_lightBlock.Position[2] = directionalLight.Z;
					_lightBlock.IsDirectional[0] = 1;

					pos2 = center + radius * new System.Numerics.Vector3(-1, 0.5f, 1);
					_lightBlock.Enabled[1] = 1;
					_lightBlock.Color[4] = color2.X;
					_lightBlock.Color[5] = color2.Y;
					_lightBlock.Color[6] = color2.Z;
					_lightBlock.Position[4] = pos2.X;
					_lightBlock.Position[5] = pos2.Y;
					_lightBlock.Position[6] = pos2.Z;

					pos3 = center + radius * new System.Numerics.Vector3(1, 0.5f, 1);
					_lightBlock.Enabled[2] = 1;
					_lightBlock.Color[8] = color3.X;
					_lightBlock.Color[9] = color3.Y;
					_lightBlock.Color[10] = color3.Z;
					_lightBlock.Position[8] = pos3.X;
					_lightBlock.Position[9] = pos3.Y;
					_lightBlock.Position[10] = pos3.Z;

					var pos4 = center + radius * new System.Numerics.Vector3(0, 0.5f, -1);
					var color4 = new Vector3(0.8f, 0.8f, 0.8f);
					_lightBlock.Enabled[3] = 1;
					_lightBlock.Color[12] = color4.X;
					_lightBlock.Color[13] = color4.Y;
					_lightBlock.Color[14] = color4.Z;
					_lightBlock.Position[12] = pos4.X;
					_lightBlock.Position[13] = pos4.Y;
					_lightBlock.Position[14] = pos4.Z;
					break;
			}
		}

		private void RecreateFrameBuffer()
		{
			_controlWidth = _control.FrameBufferWidth;
			_controlHeight = _control.FrameBufferHeight;
			if (_controlWidth > 0 && _controlHeight > 0)
			{
				_projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4,
					_controlWidth / (float)_controlHeight, 0.1f, 512.0f);
				if (_framebufferId < 0)
				{
					_framebufferId = GenFramebuffer();
					BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferId);

					_framebufferColorTexId = GenTexture();
					BindTexture(TextureTarget.Texture2DMultisample, _framebufferColorTexId);
					TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample,
						MSAA_SAMPLES, PixelInternalFormat.Rgba, _controlWidth, _controlHeight, true);
					FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, 
						TextureTarget.Texture2DMultisample, _framebufferColorTexId, 0);
					BindTexture(TextureTarget.Texture2DMultisample, 0);

					_framebufferDepthTexId = GenRenderbuffer();
					BindRenderbuffer(RenderbufferTarget.Renderbuffer, _framebufferDepthTexId);
					RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, MSAA_SAMPLES, 
						RenderbufferStorage.DepthComponent24, _controlWidth, _controlHeight);
					FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, 
						RenderbufferTarget.Renderbuffer, _framebufferDepthTexId);
					BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
				}
				else
				{
					BindTexture(TextureTarget.Texture2DMultisample, _framebufferColorTexId);
					TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample,
						MSAA_SAMPLES, PixelInternalFormat.Rgba, _controlWidth, _controlHeight, true);
					BindTexture(TextureTarget.Texture2DMultisample, 0);

					BindRenderbuffer(RenderbufferTarget.Renderbuffer, _framebufferDepthTexId);
					RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, MSAA_SAMPLES,
						RenderbufferStorage.DepthComponent24, _controlWidth, _controlHeight);
					BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
				}

				BindFramebuffer(FramebufferTarget.Framebuffer, _control.Framebuffer);
			}
		}

		public unsafe void Render(TimeSpan delta)
		{
			if (_framebufferId > 0)
			{
				BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferId);
			}
			Viewport(0, 0, _controlWidth, _controlHeight);
			Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			if (!MathHelper.ApproximatelyEqualEpsilon(_cameraDistance, _cameraDistanceInertiaTarget, 1e-5))
			{
				if (EnableInertia)
				{
					_cameraDistance += (_cameraDistanceInertiaTarget - _cameraDistance)
					                   * CAM_DISTANCE_INERTIA_FACTOR * (float)Math.Min(delta.TotalSeconds, 1);
				}
				else
				{
					_cameraDistance = _cameraDistanceInertiaTarget;
				}
			}

			if (!MathHelper.ApproximatelyEqualEpsilon(_cameraYaw, _cameraYawInertiaTarget, 1e-5) ||
			    !MathHelper.ApproximatelyEqualEpsilon(_cameraPitch, _cameraPitchInertiaTarget, 1e-5))
			{
				if (EnableInertia)
				{
					_cameraYaw += (_cameraYawInertiaTarget - _cameraYaw)
					              * CAM_MOUSEMOVE_INERTIA_FACTOR * (float)Math.Min(delta.TotalSeconds, 1);
					_cameraPitch += (_cameraPitchInertiaTarget - _cameraPitch)
					                * CAM_MOUSEMOVE_INERTIA_FACTOR * (float)Math.Min(delta.TotalSeconds, 1);
				}
				else
				{
					_cameraYawInertiaTarget = _cameraYaw;
					_cameraPitchInertiaTarget = _cameraPitch;
				}
			}
			else
			{
				if (_cameraYaw > 360)
				{
					_cameraYaw -= 360;
					_cameraYawInertiaTarget = _cameraYaw;
				}
				else if (_cameraYaw < -360)
				{
					_cameraYaw += 360;
					_cameraYawInertiaTarget = _cameraYaw;
				}
				if (_cameraPitch > 360)
				{
					_cameraPitch -= 360;
					_cameraPitchInertiaTarget = _cameraPitch;
				}
				else if (_cameraPitch < -360)
				{
					_cameraPitch += 360;
					_cameraPitchInertiaTarget = _cameraPitch;
				}
			}

			if (Vector3.Distance(_cameraAimInertiaTarget, _cameraAim) > 0.0001f)
			{
				if (EnableTransitionInertia)
				{
					_cameraAim += (_cameraAimInertiaTarget - _cameraAim) * CAM_AIM_INERTIA_FACTOR * (float)Math.Min(delta.TotalSeconds, 1);
				}
				else
				{
					_cameraAim = _cameraAimInertiaTarget;
				}
			}

			var viewMat = Matrix4.CreateTranslation(0, 0, -_cameraDistance);
			viewMat = Matrix4.CreateRotationX((float)_cameraPitch / 180f * (float)Math.PI) * viewMat;
			viewMat = Matrix4.CreateRotationY((float)_cameraYaw / 180f * (float)Math.PI) * viewMat;
			viewMat = Matrix4.CreateTranslation(_cameraAim.X, -_cameraAim.Z, -_cameraAim.Y) * viewMat;

			var worldMat = Matrix4.CreateRotationX(-90 / 180f * (float)Math.PI) *
			               Matrix4.CreateRotationY((float)Math.PI);

			var normalMat = new Matrix3(worldMat);
			normalMat.Invert();
			normalMat.Transpose();

			var vpMatrix = viewMat * _projectionMatrix;

			BindBuffer(BufferTarget.UniformBuffer, _uboLights);
			BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, sizeof(ShaderManager.LightBlock), ref _lightBlock);
			BindBuffer(BufferTarget.UniformBuffer, 0);

			foreach (var renderMesh in Meshes)
			{
				if (renderMesh.Mesh == null)
					continue;

				var shader = renderMesh.Shader ?? ShaderManager.DefaultShader;
				shader.Use();
				shader.SetViewProjectionMatrix(ref vpMatrix);
				shader.SetWorldMatrix(ref worldMat);
				shader.SetNormalMatrix(ref normalMat);
				shader.SetTexDiffuse(0);
				shader.SetLightBlock(_uboLights);

				var texture = renderMesh.Texture;

				if (texture != null)
					texture.Bind();

				if (renderMesh.DoubleSides)
					Disable(EnableCap.CullFace);
				else
					Enable(EnableCap.CullFace);

				if (renderMesh.AlphaTest)
				{
					shader.SetAlphaTest(true, renderMesh.AlphaTestValue);
				}
				else
				{
					shader.SetAlphaTest(false);
				}

				renderMesh.Mesh.Draw();

				if (texture != null)
					BindTexture(TextureTarget.Texture2D, 0);
			}

			Disable(EnableCap.CullFace);

			if (ShowGrid)
			{
				// has some bug when either GridX and GridY is odd number. make sure they are even numbers
				var x = Math.Min(GridX + (GridX & 1), 256);
				var y = Math.Min(GridY + (GridY & 1), 256);
				var xDivBy2 = x / 2;
				var yDivBy2 = y / 2;
				var shader = ShaderManager.GridShader;
				shader.Use();
				shader.SetViewProjectionMatrix(ref vpMatrix);
				shader.SetColor(new Vector4(0.2f, 0.2f, 0.2f, 1f));
				shader.SetBackColor(new Vector4(0.4f, 0.4f, 0.4f, 1f));

				if (GridX > 0)
				{
					// ReSharper disable once PossibleLossOfFraction
					worldMat = Matrix4.CreateTranslation(-xDivBy2, 0, 0);
					//worldMat = Matrix4.Identity;
					normalMat = new Matrix3(worldMat);
					normalMat.Invert();
					normalMat.Transpose();
					shader.SetWorldMatrix(ref worldMat);
					shader.SetNormalMatrix(ref normalMat);
					shader.SetGridLength(yDivBy2);
					_gridMesh.DrawInstanced(x + 1);
				}

				if (GridY > 0)
				{
					// ReSharper disable once PossibleLossOfFraction
					worldMat = Matrix4.CreateTranslation(-yDivBy2, 0, 0) * Matrix4.CreateRotationY(MathHelper.PiOver2);
					//worldMat = Matrix4.Identity;
					normalMat = new Matrix3(worldMat);
					normalMat.Invert();
					normalMat.Transpose();
					shader.SetWorldMatrix(ref worldMat);
					shader.SetNormalMatrix(ref normalMat);
					shader.SetGridLength(xDivBy2);
					_gridMesh.DrawInstanced(y + 1);
				}
			}

			Enable(EnableCap.CullFace);

			vpMatrix = Matrix4.CreateOrthographic(2, 2, -1, 1);
			worldMat = Matrix4.CreateScale(0.05f) * Matrix4.CreateTranslation(0.9f, -0.9f, 0);
			worldMat = Matrix4.CreateRotationX((float)_cameraPitch / 180f * (float)Math.PI) * worldMat;
			worldMat = Matrix4.CreateRotationY((float)_cameraYaw / 180f * (float)Math.PI) * worldMat;
			var ds = ShaderManager.DefaultShader;
			ds.Use();
			ds.SetWorldMatrix(ref worldMat);
			ds.SetViewProjectionMatrix(ref vpMatrix);
			ds.SetColor(new Vector4(62 / 255f, 62 / 255f, 124 / 255f, 1));
			_arrowMesh.Draw();

			var worldMatX = Matrix4.CreateRotationZ(MathHelper.PiOver2) * worldMat;
			ds.SetWorldMatrix(ref worldMatX);
			ds.SetColor(new Vector4(124 / 255f, 62 / 255f, 62 / 255f, 1));
			_arrowMesh.Draw();

			var worldMatY = Matrix4.CreateRotationX(MathHelper.PiOver2) * worldMat;
			ds.SetWorldMatrix(ref worldMatY);
			ds.SetColor(new Vector4(62 / 255f, 124 / 255f, 62 / 255f, 1));
			_arrowMesh.Draw();

			Flush();

			if (_framebufferId > 0)
			{
				BindFramebuffer(FramebufferTarget.Framebuffer, _control.Framebuffer);
				BindFramebuffer(FramebufferTarget.DrawFramebuffer, _control.Framebuffer);
				BindFramebuffer(FramebufferTarget.ReadFramebuffer, _framebufferId);
				BlitFramebuffer(0, 0, _controlWidth, _controlHeight, 0, 0, _controlWidth, _controlHeight,
					ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);

				Flush();
			}
		}

		public struct RenderMesh
		{
			public MeshManager.OglMesh Mesh;

			public TextureManager.OglTexture Texture;

			public ShaderManager.Shader Shader;

			public bool AlphaTest;

			public bool DoubleSides;

			public float AlphaTestValue;
		}

		public void RenderImage(TextureManager.OglTexture texture)
		{
			Viewport(0, 0, _controlWidth, _controlHeight);
			Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			if (_controlWidth > _controlHeight)
			{
				var spare = _controlWidth - _controlHeight;
				var leftMargin = spare / 2;
				Viewport(leftMargin, 0, _controlWidth - spare, _controlHeight);
			}
			else
			{
				var spare = _controlHeight - _controlWidth;
				var bottomMargin = spare / 2;
				Viewport(0, bottomMargin, _controlWidth, _controlHeight - spare);
			}

			if (texture != null)
			{
				var matrix = Matrix4.CreateOrthographic(2, 2, -1, 1);
				var imageColorMask = ImageColorMask;

				var shader = ShaderManager.ImageShader;
				shader.Use();
				shader.SetViewProjectionMatrix(ref matrix);
				shader.SetColorMask(ref imageColorMask);
				shader.SetTexDiffuse(0);

				texture.Bind();

				if (EnableAlpha)
				{
					Enable(EnableCap.Blend);
					BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
					shader.SetAlphaTest(true);
				}
				else
				{
					Disable(EnableCap.Blend);
					shader.SetAlphaTest(false);
				}

				_imageMesh.Draw();

				BindTexture(TextureTarget.Texture2D, 0);
			}

			Flush();
		}

		private static float[] ARROW_MESH_POS = new[]
		{
			0.000000f, 0.000000f, -0.100000f,
			0.000000f, 1.000119f, -0.100000f,
			0.070711f, 0.000000f, -0.070711f,
			0.070711f, 1.000119f, -0.070711f,
			0.100000f, 0.000000f, 0.000000f,
			0.100000f, 1.000119f, 0.000000f,
			0.070711f, 0.000000f, 0.070711f,
			0.070711f, 1.000119f, 0.070711f,
			-0.000000f, 0.000000f, 0.100000f,
			-0.000000f, 1.000119f, 0.100000f,
			-0.070711f, 0.000000f, 0.070711f,
			-0.070711f, 1.000119f, 0.070711f,
			-0.100000f, 0.000000f, -0.000000f,
			-0.100000f, 1.000119f, -0.000000f,
			-0.070711f, 0.000000f, -0.070711f,
			-0.070711f, 1.000119f, -0.070711f,
			0.142461f, 1.000119f, -0.142461f,
			0.000000f, 1.000119f, -0.201470f,
			0.201470f, 1.000119f, 0.000000f,
			0.142461f, 1.000119f, 0.142461f,
			-0.000000f, 1.000119f, 0.201470f,
			-0.142461f, 1.000119f, 0.142461f,
			-0.201470f, 1.000119f, -0.000000f,
			-0.142461f, 1.000119f, -0.142461f,
			0.000000f, 1.641037f, 0.000000f,
			0.000000f, 0.000000f, -0.100000f,
			0.070711f, 0.000000f, -0.070711f,
			0.070711f, 1.000119f, -0.070711f,
			0.000000f, 1.000119f, -0.100000f,
			0.100000f, 0.000000f, 0.000000f,
			0.100000f, 1.000119f, 0.000000f,
			0.070711f, 0.000000f, 0.070711f,
			0.070711f, 1.000119f, 0.070711f,
			-0.000000f, 0.000000f, 0.100000f,
			-0.000000f, 1.000119f, 0.100000f,
			-0.070711f, 0.000000f, 0.070711f,
			-0.070711f, 1.000119f, 0.070711f,
			-0.100000f, 0.000000f, -0.000000f,
			-0.100000f, 1.000119f, -0.000000f,
			-0.070711f, 0.000000f, -0.070711f,
			-0.070711f, 1.000119f, -0.070711f,
			0.142461f, 1.000119f, -0.142461f,
			0.000000f, 1.000119f, -0.201470f,
			0.201470f, 1.000119f, 0.000000f,
			0.142461f, 1.000119f, 0.142461f,
			-0.000000f, 1.000119f, 0.201470f,
			-0.142461f, 1.000119f, 0.142461f,
			-0.201470f, 1.000119f, -0.000000f,
			-0.142461f, 1.000119f, -0.142461f
		};

		private static int[] ARROW_MESH_INDEX = new[]
		{
			29, 27, 26,
			28, 30, 27,
			31, 32, 30,
			33, 34, 32,
			35, 36, 34,
			37, 38, 36,
			12, 48, 14,
			39, 40, 38,
			41, 26, 40,
			7, 11, 15,
			47, 21, 25,
			8, 46, 10,
			16, 48, 49,
			4, 44, 6,
			10, 22, 12,
			16, 43, 2,
			2, 42, 4,
			6, 45, 8,
			20, 19, 25,
			17, 18, 25,
			18, 24, 25,
			23, 47, 25,
			21, 20, 25,
			19, 17, 25,
			24, 23, 25,
			29, 28, 27,
			28, 31, 30,
			31, 33, 32,
			33, 35, 34,
			35, 37, 36,
			37, 39, 38,
			12, 22, 48,
			39, 41, 40,
			41, 29, 26,
			15, 1, 3,
			3, 5, 7,
			7, 9, 11,
			11, 13, 15,
			15, 3, 7,
			8, 45, 46,
			16, 14, 48,
			4, 42, 44,
			10, 46, 22,
			16, 49, 43,
			2, 43, 42,
			6, 44, 45
		};
	}
}