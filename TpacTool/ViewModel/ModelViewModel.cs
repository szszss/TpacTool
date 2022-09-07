using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using TpacTool.IO;
using TpacTool.IO.Assimp;
using TpacTool.Lib;
using TpacTool.Properties;
using Material = TpacTool.Lib.Material;

namespace TpacTool
{
	public class ModelViewModel : ViewModelBase
	{
		public static readonly Guid UpdateSkeletonListEvent = Guid.NewGuid();

		public static readonly Guid UpdateModelListEvent = Guid.NewGuid();

		private List<Skeleton> _skeletons = new List<Skeleton>();

		private Skeleton _human_skeleton;

		private Skeleton _horse_skeleton;

		internal static bool _model_exporter_unavailable = false;

		private SaveFileDialog _saveFileDialog;

		public Metamesh Asset { private set; get; }

		private SkeletonType _skeletonType = SkeletonType.Human;

		//private MaterialExportSetting materialExport = MaterialExportSetting.Export;
		private int _selectedLod = 0;
		private int _selectedMeshIndex;
		private Mesh _selectedMesh;
		private bool _isRigged;

		public SkeletonType ExportSkeletonType
		{
			set => _skeletonType = value;
			get => _skeletonType;
		}

		public bool IsExportOnlyLod0
		{
			set => Settings.Default.ExportModelAllLods = !value;
			get => !Settings.Default.ExportModelAllLods;
		}

		public bool IsExportAllLods
		{
			set => Settings.Default.ExportModelAllLods = value;
			get => Settings.Default.ExportModelAllLods;
		}

		public bool IsSkeletonOtherAndRigged => _skeletonType == SkeletonType.Other && ExportAsRigged;

		public bool IsMaterialIgnored => Settings.Default.ExportModelTexture == (int)MaterialExportSetting.None;

		public bool IsMaterialExportToSameFolder => Settings.Default.ExportModelTexture == (int) MaterialExportSetting.Export;

		public bool IsMaterialExportToSubFolder => Settings.Default.ExportModelTexture == (int)MaterialExportSetting.ExportToSubFolder;

		public string[] PreferredFormatItems => MaterialViewModel._preferredFormatItems;

		public int PreferredFormat { set; get; } = 0;

		public int SelectedLod
		{
			set
			{
				_selectedLod = value;
				int lod = LodSteps[_selectedLod];
				MessengerInstance.Send(Asset.Meshes.FindAll(mesh => mesh.Lod == lod),
					OglPreviewViewModel.PreviewAssetEvent);
				RaisePropertyChanged("SelectedLodValue");
			}
			get => _selectedLod;
		}

		public int SelectedLodValue => LodSteps[SelectedLod];

		public int LodCount { private set; get; }

		public int[] LodSteps { private set; get; }

		public int SelectedMeshIndex
		{
			set
			{
				_selectedMeshIndex = value;
				if (_selectedMeshIndex >= 0)
				{
					SelectedMesh = Asset.Meshes[_selectedMeshIndex];
					RaisePropertyChanged("SelectedMesh");
				}
			}
			get => _selectedMeshIndex;
		}

		public Mesh SelectedMesh
		{
			private set
			{
				_selectedMesh = value;
				if (_selectedMesh != null)
				{
					SelectedMeshPrimaryMaterial =
						_selectedMesh.Material.IsEmpty() ? 
							null : _selectedMesh.Material.GetItem();
					SelectedMeshSecondMaterial =
						_selectedMesh.SecondMaterial.IsEmpty() ? 
							null : _selectedMesh.SecondMaterial.GetItem();
					IsRigged = _selectedMesh.SkinDataSize > 0;
					HasMorph = _selectedMesh.EditData != null
						? _selectedMesh.EditData.Data.MorphFrames.Count > 0
						: false;
				}
				else
				{
					SelectedMeshPrimaryMaterial = null;
					SelectedMeshSecondMaterial = null;
					IsRigged = false;
					HasMorph = false;
				}
				RaisePropertyChanged("SelectedMeshPrimaryMaterial");
				RaisePropertyChanged("SelectedMeshSecondMaterial");
				RaisePropertyChanged("IsRigged");
				RaisePropertyChanged("HasMorph");
			}
			get => _selectedMesh;
		}

		public Material SelectedMeshPrimaryMaterial { private set; get; }

		public Material SelectedMeshSecondMaterial { private set; get; }

		public bool IsRigged
		{
			private set
			{
				_isRigged = value;
				if (!_isRigged)
				{
					ExportAsRigged = false;
					RaisePropertyChanged("ExportAsRigged");
				}
			}
			get => _isRigged;
		}

		public bool CanExport => !_model_exporter_unavailable && Asset != null;

		public bool HasMorph { private set; get; }

		public bool ExportAsRigged { private set; get; }

		public bool FixBlenderBone { set; get; } = false;

		public bool OnlyExportDiffuse
		{
			set => Settings.Default.ExportModelDiffuseOnly = value;
			get => Settings.Default.ExportModelDiffuseOnly;
		}

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

		public bool UseYUpAxis
		{
			set => Settings.Default.ExportModelObjYUp = value;
			get => Settings.Default.ExportModelObjYUp;
		}

		public ICommand ChangeSkeletonCommand { private set; get; }

		public ICommand ChangeMaterialCommand { private set; get; }

		public ICommand ChangeRiggedCommand { private set; get; }

		public ICommand ExportCommand { private set; get; }

		public List<Skeleton> Skeletons => _skeletons;

		public int SelectedSkeletonIndex { set; get; } = -1;

		public ModelViewModel()
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
					_saveFileDialog.Filter = "Wavefront OBJ (*.obj)|*.obj|" +
					                         "Autodesk FBX (*.fbx)|*.fbx|" +
					                         "COLLADA (*.dae)|*.dae";
					_saveFileDialog.FilterIndex = 2;
				}
				else
				{
					MessageBox.Show(Resources.Msgbox_AssimpNotFound, Resources.Msgbox_Warning,
						MessageBoxButton.OK, MessageBoxImage.Warning);

					_saveFileDialog.Filter = "Wavefront OBJ (*.obj)|*.obj|" +
					                         "COLLADA (*.dae)|*.dae";
					_saveFileDialog.FilterIndex = 2;
				}
				_saveFileDialog.Title = Resources.Model_Dialog_SelectExportFile;

				ChangeSkeletonCommand = new RelayCommand<string>(arg =>
				{
					SkeletonType.TryParse(arg, true, out SkeletonType result);
					_skeletonType = result;
					RaisePropertyChanged("IsSkeletonHuman");
					RaisePropertyChanged("IsSkeletonHorse");
					RaisePropertyChanged("IsSkeletonOther");
					RaisePropertyChanged("IsSkeletonOtherAndRigged");
				});

				ChangeMaterialCommand = new RelayCommand<string>(arg =>
				{
					MaterialExportSetting.TryParse(arg, true, out MaterialExportSetting result);
					Settings.Default.ExportModelTexture = (int) result;
					RaisePropertyChanged("IsMaterialIgnored");
					RaisePropertyChanged("IsMaterialExportToSameFolder");
					RaisePropertyChanged("IsMaterialExportToSubFolder");
				});

				ChangeRiggedCommand = new RelayCommand<string>(arg =>
				{
					ExportAsRigged = bool.Parse(arg);
					RaisePropertyChanged("ExportAsRigged");
					RaisePropertyChanged("IsSkeletonOtherAndRigged");
				});

				ExportCommand = new RelayCommand(Export);

				MessengerInstance.Register<AssetItem>(this, AssetTreeViewModel.AssetSelectedEvent, OnSelectAsset);
				MessengerInstance.Register<IEnumerable<Skeleton>>(this, UpdateSkeletonListEvent, skeletons =>
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
					RaisePropertyChanged("Skeletons");
					//SelectedSkeletonIndex = 0;
					//RaisePropertyChanged("SelectedSkeletonIndex");
				});
				MessengerInstance.Register<object>(this, MainViewModel.CleanupEvent, unused =>
				{
					_skeletons.Clear();
					_human_skeleton = null;
					_horse_skeleton = null;
					SelectedMesh = null;
					SelectedMeshPrimaryMaterial = null;
					SelectedMeshSecondMaterial = null;
					Asset = null;
					RaisePropertyChanged("SelectedMesh");
					RaisePropertyChanged("SelectedMeshPrimaryMaterial");
					RaisePropertyChanged("SelectedMeshSecondMaterial");
					RaisePropertyChanged("Asset");
				});
			}
		}

		private void OnSelectAsset(AssetItem assetItem)
		{
			var metamesh = assetItem as Metamesh;
			if (metamesh != null)
			{
				Asset = metamesh;
				var set = new SortedSet<int>();
				foreach (var mesh in metamesh.Meshes)
				{
					set.Add(mesh.Lod);
				}

				LodSteps = set.ToArray();
				LodCount = LodSteps.Length - 1;
				SelectedLod = 0;
				SelectedMeshIndex = 0;
				RaisePropertyChanged("Asset");
				RaisePropertyChanged("LodCount");
				RaisePropertyChanged("SelectedLod");
				RaisePropertyChanged("SelectedMeshIndex");
			}
		}

		private void Export()
		{
			if (!CanExport)
				return;

			if (IsSkeletonOtherAndRigged && SelectedSkeletonIndex < 0)
			{
				MessageBox.Show(Resources.Msgbox_SkeletonNotSelected,
					Resources.Msgbox_Error, MessageBoxButton.OK, MessageBoxImage.Stop);
				return;
			}

			Skeleton skeleton = null;
			if (ExportAsRigged)
			{
				switch (_skeletonType)
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
						skeleton = Skeletons[SelectedSkeletonIndex];
						break;
				}
			}

			ModelExporter.ModelExportOption option = 0;

			if (UseLargerScale)
				option |= ModelExporter.ModelExportOption.LargerSize;
			if (UseNegYForwardAxis)
				option |= ModelExporter.ModelExportOption.NegYAxisForward;
			if (UseYUpAxis)
				option |= ModelExporter.ModelExportOption.YAxisUp;
			if (FixBlenderBone)
				option |= ModelExporter.ModelExportOption.FixBoneForBlender;
			if (IsMaterialExportToSameFolder)
				option |= ModelExporter.ModelExportOption.ExportTextures;
			else if (IsMaterialExportToSubFolder)
				option |= ModelExporter.ModelExportOption.ExportTexturesSubFolder;
			if (OnlyExportDiffuse)
				option |= ModelExporter.ModelExportOption.ExportDiffuseOnly;
			if (IsExportAllLods)
				option |= ModelExporter.ModelExportOption.ExportAllLod;

			_saveFileDialog.FileName = Asset.Name;
			if (_saveFileDialog.ShowDialog().GetValueOrDefault(false))
			{
				var path = _saveFileDialog.FileName;

				MessengerInstance.Send(string.Format("Export {0} ...", Asset.Name), MainViewModel.StatusEvent);
				if (AssimpModelExporter.IsAssimpAvailable())
					AssimpModelExporter.ExportToFile(path, Asset, skeleton, option);
				else
					ModelExporter.ExportToFile(path, Asset, skeleton, option);
				MessengerInstance.Send(string.Format("{0} exported", Asset.Name), MainViewModel.StatusEvent);
			}
		}

		private enum MaterialExportSetting
		{
			None = 0,
			Export = 1,
			ExportToSubFolder = 2
		}
	}
}