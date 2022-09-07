using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using TpacTool.IO;
using TpacTool.IO.Assimp;
using TpacTool.Lib;
using TpacTool.Properties;

namespace TpacTool
{
	public class AnimationViewModel : ViewModelBase
	{
		private List<Skeleton> _skeletons = new List<Skeleton>();

		private Skeleton _human_skeleton;

		private Skeleton _horse_skeleton;

		public List<Skeleton> Skeletons => _skeletons;

		public int SelectedSkeletonIndex { set; get; } = -1;

		private List<Metamesh> _unfilteredModels = new List<Metamesh>();

		private List<Metamesh> _models = new List<Metamesh>();

		private Metamesh _male_head;

		private Metamesh _female_head;

		public List<Metamesh> Models => _models;

		public int SelectedModelIndex { set; get; } = -1;

		public ICommand ChangeSkeletonCommand { private set; get; }

		public ICommand ChangeModelCommand { private set; get; }

		public ICommand ChangeFrameCommand { private set; get; }

		public ICommand ExportAnimationCommand { private set; get; }

		public ICommand ExportMorphCommand { private set; get; }

		private SaveFileDialog _saveFileDialog;

		private SkeletalAnimation _animationAsset;

		private MorphAnimation _morphAsset;

		private SkeletonType _exportedSkeletonType = SkeletonType.Default;

		private ModelType _exportedModelType = ModelType.MaleHead;

		public SkeletalAnimation AnimationAsset
		{
			set
			{
				if (value != null && value.Skeleton != Guid.Empty)
					DefaultSkeleton = _skeletons.Find(skeleton => skeleton.Guid == value.Skeleton);
				else
					DefaultSkeleton = null;
				_animationAsset = value;
				RaisePropertyChanged(nameof(DefaultSkeleton));
				RaisePropertyChanged(nameof(AnimationSkeletonName));
				RaisePropertyChanged(nameof(AnimationBoneCount));
			}
			get => _animationAsset;
		}

		public MorphAnimation MorphAsset
		{
			set
			{
				_morphAsset = value;
			}
			get => _morphAsset;
		}

		public Skeleton DefaultSkeleton { private set; get; }

		public string AnimationSkeletonName
		{
			get
			{
				if (DefaultSkeleton != null)
					return DefaultSkeleton.Name;
				return String.Empty;
			}
		}

		public string AnimationBoneCount
		{
			get
			{
				if (_animationAsset != null)
					return _animationAsset.BoneNum.ToString();
				return String.Empty;
			}
		}

		public float FrameRate
		{
			set
			{
				Settings.Default.ExportAnimationFrameRate = Math.Min(Math.Max(value, 1f), 60f);
			}

			get
			{
				return Settings.Default.ExportAnimationFrameRate;
			}
		}

		public bool IsExportDefaultSkeleton => _exportedSkeletonType == SkeletonType.Default;

		public bool IsExportHumanSkeleton => _exportedSkeletonType == SkeletonType.Human;

		public bool IsExportHorseSkeleton => _exportedSkeletonType == SkeletonType.Horse;

		public bool IsExportOtherSkeleton => _exportedSkeletonType == SkeletonType.Other;

		public bool IsExportMaleHeadModel => _exportedModelType == ModelType.MaleHead;

		public bool IsExportFemaleHeadModel => _exportedModelType == ModelType.FemaleHead;

		public bool IsExportOtherHeadModel => _exportedModelType == ModelType.Other;

		public bool UseLargerScale
		{
			set => Settings.Default.ExportModelLargerScale = value;
			get => Settings.Default.ExportModelLargerScale;
		}

		public bool UseNegYForwardAxis
		{
			set => Settings.Default.ExportModelNegYForward = value;
			get => Settings.Default.ExportModelNegYForward;
		}

		public bool UseAscii
		{
			set => Settings.Default.ExportModelFbxAscii = value;
			get => Settings.Default.ExportModelFbxAscii;
		}

		public SkeletonType ExportedSkeletonType
		{
			set
			{
				_exportedSkeletonType = value;
				RaisePropertyChanged(nameof(ExportedSkeletonType));
				RaisePropertyChanged(nameof(IsExportOtherSkeleton));
			}
			get => _exportedSkeletonType;
		}

		public ModelType ExportedModelType
		{
			set
			{
				_exportedModelType = value;
				if (value == ModelType.Other && _models.Count == 0 && _unfilteredModels.Count > 0)
				{
					_models.AddRange(_unfilteredModels
						.Where(model => model.Meshes.Any(mesh => mesh.VertexKeyCount > 0)));
					_unfilteredModels.Clear();
				}
				RaisePropertyChanged(nameof(Models));
				RaisePropertyChanged(nameof(ExportedModelType));
				RaisePropertyChanged(nameof(IsExportOtherHeadModel));
			}
			get => _exportedModelType;
		}

		public bool CanExport => AssimpModelExporter.IsAssimpAvailable();

		public AnimationViewModel()
		{
			if (IsInDesignMode)
			{
			}
			else
			{
				_saveFileDialog = new SaveFileDialog();
				_saveFileDialog.CreatePrompt = false;
				_saveFileDialog.OverwritePrompt = true;
				_saveFileDialog.AddExtension = true;
				if (AssimpModelExporter.IsAssimpAvailable())
				{
					_saveFileDialog.Filter = "Autodesk FBX (*.fbx)|*.fbx";
					_saveFileDialog.FilterIndex = 1;
				}
				else
				{
					//_saveFileDialog.Filter = "COLLADA (*.dae)|*.dae";
					//_saveFileDialog.FilterIndex = 1;
				}
				_saveFileDialog.Title = Resources.Model_Dialog_SelectExportFile;

				ChangeSkeletonCommand = new RelayCommand<string>(arg =>
				{
					SkeletonType.TryParse(arg, true, out SkeletonType result);
					ExportedSkeletonType = result;
				});

				ChangeModelCommand = new RelayCommand<string>(arg =>
				{
					ModelType.TryParse(arg, true, out ModelType result);
					ExportedModelType = result;
				});

				ChangeFrameCommand = new RelayCommand<string>(arg =>
				{
					float frame = 24f;
					if (float.TryParse(arg, out var value))
					{
						frame = value;
					}

					FrameRate = frame;
					RaisePropertyChanged(nameof(FrameRate));
				});

				ExportAnimationCommand = new RelayCommand(ExportAnimation);

				ExportMorphCommand = new RelayCommand(ExportMorph);

				MessengerInstance.Register<AssetItem>(this, AssetTreeViewModel.AssetSelectedEvent, asset =>
				{
					if (asset is SkeletalAnimation animation)
						AnimationAsset = animation;
					else if (asset is MorphAnimation morph)
						MorphAsset = morph;
				});

				MessengerInstance.Register<IEnumerable<Skeleton>>(this, ModelViewModel.UpdateSkeletonListEvent, skeletons =>
				{
					_skeletons.Clear();
					_skeletons.AddRange(skeletons);

					foreach (var skeleton in _skeletons)
					{
						if (skeleton.Name == "human_skeleton")
							_human_skeleton = skeleton;
						else if (skeleton.Name == "horse_skeleton")
							_horse_skeleton = skeleton;
					}
					RaisePropertyChanged(nameof(Skeletons));
				});

				MessengerInstance.Register<IEnumerable<Metamesh>>(this, ModelViewModel.UpdateModelListEvent, models =>
				{
					_models.Clear();
					_unfilteredModels.Clear();
					_unfilteredModels.AddRange(models);

					foreach (var model in _unfilteredModels)
					{
						if (model.Name == "head_male_a")
							_male_head = model;
						else if (model.Name == "head_female_a")
							_female_head = model;
					}
					RaisePropertyChanged(nameof(Models));
				});

				MessengerInstance.Register<object>(this, MainViewModel.CleanupEvent, unused =>
				{
					_skeletons.Clear();
					_human_skeleton = null;
					_horse_skeleton = null;
					AnimationAsset = null;
					MorphAsset = null;
					_models.Clear();
					_unfilteredModels.Clear();
					_male_head = null;
					_female_head = null;
					RaisePropertyChanged(nameof(Skeletons));
					RaisePropertyChanged(nameof(Models));
					RaisePropertyChanged(nameof(AnimationAsset));
				});
			}
		}

		public void ExportAnimation()
		{
			Export(true, false);
		}

		public void ExportMorph()
		{
			Export(false, true);
		}

		private void Export(bool exportAnimation, bool exportMorph)
		{
			if (!AssimpModelExporter.IsAssimpAvailable())
			{
				MessageBox.Show(Resources.Msgbox_AnimationAssimpRequired,
					Resources.Msgbox_Error, MessageBoxButton.OK, MessageBoxImage.Stop);
				return;
			}

			Skeleton skeleton = null;

			if (exportAnimation)
			{
				switch (_exportedSkeletonType)
				{
					case SkeletonType.Human:
						if (_human_skeleton == null)
						{
							MessageBox.Show(Resources.Msgbox_HumanSkeletonNotFound,
								Resources.Msgbox_Error, MessageBoxButton.OK, MessageBoxImage.Stop);
							return;
						}
						skeleton = _human_skeleton;
						break;
					case SkeletonType.Horse:
						if (_horse_skeleton == null)
						{
							MessageBox.Show(Resources.Msgbox_HorseSkeletonNotFound,
								Resources.Msgbox_Error, MessageBoxButton.OK, MessageBoxImage.Stop);
							return;
						}
						skeleton = _horse_skeleton;
						break;
					case SkeletonType.Other:
						if (SelectedSkeletonIndex < 0)
						{
							MessageBox.Show(Resources.Msgbox_SkeletonNotSelected,
								Resources.Msgbox_Error, MessageBoxButton.OK, MessageBoxImage.Stop);
							return;
						}
						skeleton = Skeletons[SelectedSkeletonIndex];
						break;
					case SkeletonType.Default:
						if (DefaultSkeleton == null)
						{
							MessageBox.Show(Resources.Msgbox_DefaultSkeletonNotFound,
								Resources.Msgbox_Error, MessageBoxButton.OK, MessageBoxImage.Stop);
							return;
						}
						skeleton = DefaultSkeleton;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			Metamesh model = null;
			if (exportMorph)
			{
				switch (_exportedModelType)
				{
					case ModelType.MaleHead:
						if (_male_head == null)
						{
							MessageBox.Show(Resources.Msgbox_MaleHeadModelNotFound,
								Resources.Msgbox_Error, MessageBoxButton.OK, MessageBoxImage.Stop);
							return;
						}
						model = _male_head;
						break;
					case ModelType.FemaleHead:
						if (_female_head == null)
						{
							MessageBox.Show(Resources.Msgbox_FemaleHeadModelNotFound,
								Resources.Msgbox_Error, MessageBoxButton.OK, MessageBoxImage.Stop);
							return;
						}
						model = _female_head;
						break;
					case ModelType.Other:
						if (SelectedModelIndex < 0)
						{
							MessageBox.Show(Resources.Msgbox_ModelNotSelected,
								Resources.Msgbox_Error, MessageBoxButton.OK, MessageBoxImage.Stop);
							return;
						}
						model = Models[SelectedModelIndex];
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			ModelExporter.ModelExportOption option = 0;
			AssimpModelExporter.AssimpModelExportOption assimpOption = 0;

			if (UseLargerScale)
				option |= ModelExporter.ModelExportOption.LargerSize;
			if (UseNegYForwardAxis)
				option |= ModelExporter.ModelExportOption.NegYAxisForward;
			if (UseAscii)
				assimpOption |= AssimpModelExporter.AssimpModelExportOption.UseAscii;

			SkeletalAnimation animation = exportAnimation ? AnimationAsset : null;
			MorphAnimation morph = exportMorph ? MorphAsset : null;
			var assetName = morph != null ? morph.Name : animation?.Name ?? "";

			_saveFileDialog.FileName = assetName;
			if (_saveFileDialog.ShowDialog().GetValueOrDefault(false))
			{
				var path = _saveFileDialog.FileName;

				MessengerInstance.Send(string.Format("Export {0} ...", assetName), MainViewModel.StatusEvent);
				if (AssimpModelExporter.IsAssimpAvailable())
				{
					AssimpModelExporter.ExportToFile(path, model, skeleton, animation, morph, option, assimpOption, FrameRate);
				}
				MessengerInstance.Send(string.Format("{0} exported", assetName), MainViewModel.StatusEvent);
			}
		}
	}
}