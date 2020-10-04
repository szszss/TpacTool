using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using TpacTool.Lib;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

namespace TpacTool
{
	public class ModelPreviewViewModel : ViewModelBase
	{
		public static readonly Guid PreviewModelEvent = Guid.NewGuid();

		public MatrixCamera Camera { private set; get; }

		public double CameraYaw { set; get; }

		public double CameraPitch { set; get; }

		public double CameraDistance { set; get; }

		private Matrix3D _projectionMatrix;

		private Matrix3D _viewMatrix;

		private double YawFactor = 0.5;

		private double PitchFactor = 0.5;

		private double DistanceFactor = 0.0035;

		private double WheelMode = -1;

		public Model3DGroup Models { set; get; }

		public ModelPreviewViewModel()
		{
			this.Camera = new MatrixCamera();
			UpdateProjectionMatrix(1);
			CameraYaw = -45;
			CameraPitch = 45;
			CameraDistance = 6;
			UpdateViewMatrix();
			Models = new Model3DGroup();

			if (!IsInDesignMode)
			{
				//_messenger = SimpleIoc.Default.GetInstance<Messenger>(MESSENGER_KEY);
				//_messenger.Register<Metamesh>(this, OnSelectAsset);
				MessengerInstance.Register<List<Mesh>>(this, PreviewModelEvent, OnPreviewModel);
				MessengerInstance.Register<object>(this, MainViewModel.CleanupEvent, unused =>
				{
					Models.Children.Clear();
					RaisePropertyChanged("Models");
				});
			}
		}

		private void OnPreviewModel(List<Mesh> meshes)
		{
			SetRenderMeshes(meshes.ToArray());
		}

		/*private void OnSelectAsset(AssetItem assetItem)
		{
			var metamesh = assetItem as Metamesh;
			if (metamesh != null)
				SetRenderMetamesh(metamesh);
		}*/

		public void SetRenderMetamesh(Metamesh metamesh, int lod = 0)
		{
			var meshes = metamesh.Meshes.FindAll(mesh => { return mesh.Lod == lod; });
			SetRenderMeshes(meshes.ToArray());
		}

		public void SetRenderMeshes(params Mesh[] meshes)
		{
			Models.Children.Clear();
			foreach (var mesh in meshes)
			{
				Models.Children.Add(ResourceCache.GetModel(mesh).Mesh);
			}
            if (Models.Children.First() != null) {
                var target = Models.Children.First().Bounds.Location;
                //Creates a fake ambient light that does not burn corners.
                Models.Children.Add(new PointLight(Colors.White, new Point3D(50, 50, 0)));
                Models.Children.Add(new PointLight(Colors.White, new Point3D(-50, -50, 0)));
            } else {
                Models.Children.Add(new AmbientLight(Colors.White));
            }
			RaisePropertyChanged("Models");
		}

		public void UpdateMouse(double moveX, double moveY, float wheel)
		{
			CameraYaw += moveX * YawFactor;
			CameraPitch += moveY * PitchFactor;
			CameraPitch = Math.Max(Math.Min(CameraPitch, 90d), -90d);
			CameraDistance += wheel * DistanceFactor * WheelMode;
			CameraDistance = Math.Max(Math.Min(CameraDistance, 100d), 1d);
			UpdateViewMatrix();
		}

		public void UpdateProjectionMatrix(double aspectRatio)
		{
			Matrix4x4 projMat = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)aspectRatio, 0.01f, 100);
			_projectionMatrix = new Matrix3D(projMat.M11, projMat.M12, projMat.M13, projMat.M14,
				projMat.M21, projMat.M22, projMat.M23, projMat.M24,
				projMat.M31, projMat.M32, projMat.M33, projMat.M34,
				projMat.M41, projMat.M42, projMat.M43, projMat.M44);
			UpdateCamera();
		}

		public void UpdateViewMatrix()
		{
			_viewMatrix.SetIdentity();
			// fix fucking crazy z-up
			Quaternion zupfix_x = new Quaternion(new Vector3D(1, 0, 0), -90);
			Quaternion zupfix_y = new Quaternion(new Vector3D(0, 1, 0), 180);
			_viewMatrix.Rotate(zupfix_x);
			_viewMatrix.Rotate(zupfix_y);
			Quaternion quat1 = new Quaternion(new Vector3D(1, 0, 0), CameraPitch);
			Quaternion quat2 = new Quaternion(new Vector3D(0, 1, 0), CameraYaw);
			_viewMatrix.Rotate(quat2);
			_viewMatrix.Rotate(quat1);
            //center viewport on model center
			_viewMatrix.Translate(new Vector3D(0, -1, -CameraDistance));
			UpdateCamera();
		}

		private void UpdateCamera()
		{
			Camera.ProjectionMatrix = _projectionMatrix;
			Camera.ViewMatrix = _viewMatrix;
			//RaisePropertyChanged("Camera");
		}
	}
}