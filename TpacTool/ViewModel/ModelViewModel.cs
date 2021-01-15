using System;
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
using TpacTool.Lib;
using TpacTool.Properties;
using Material = TpacTool.Lib.Material;

namespace TpacTool
{
	public class ModelViewModel : ViewModelBase
	{
		public static readonly Guid UpdateSkeletonListEvent = Guid.NewGuid();

		internal static List<Skeleton> _skeletons = new List<Skeleton>();

		internal static Skeleton _human_skeleton;

		internal static Skeleton _horse_skeleton;

		internal static bool _model_exporter_unavailable = false;

		private SaveFileDialog _saveFileDialog;

		public Metamesh Asset { private set; get; }

		private SkeletonType skeletonType = SkeletonType.Human;

		private MaterialExportSetting materialExport = MaterialExportSetting.Export;
		private int _selectedLod = 0;
		private int _selectedMeshIndex;
		private Mesh _selectedMesh;
		private bool _isRigged;

		public bool IsSkeletonHuman
		{
			get => skeletonType == SkeletonType.Human;
		}

		public bool IsSkeletonHorse
		{
			get => skeletonType == SkeletonType.Horse;
		}

		public bool IsSkeletonOther
		{
			get => skeletonType == SkeletonType.Other;
		}

		public bool IsSkeletonOtherAndRigged
		{
			get => IsSkeletonOther && ExportAsRigged;
		}

		public bool IsMaterialIgnored
		{
			get => materialExport == MaterialExportSetting.None;
		}

		public bool IsMaterialExportToSameFolder
		{
			get => materialExport == MaterialExportSetting.Export;
		}

		public bool IsMaterialExportToSubFolder
		{
			get => materialExport == MaterialExportSetting.ExportToSubFolder;
		}

		public string[] PreferredFormatItems => MaterialViewModel._preferredFormatItems;

		public int PreferredFormat { set; get; } = 0;

		// some stub data
		//public int MinLod { private set; get; } = 0;

		//public int MaxLod { private set; get; } = 1;

		public int SelectedLod
		{
			set
			{
				_selectedLod = value;
				int lod = LodSteps[_selectedLod];
				MessengerInstance.Send(Asset.Meshes.FindAll(mesh => mesh.Lod == lod),
					AbstractPreviewViewModel.PreviewAssetEvent);
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

		public bool OnlyExportDiffuse { set; get; } = false;

		public bool UseLargerScale { set; get; } = false;

		public bool UseNegYForwardAxis { set; get; } = true;

		public bool UseYUpAxis { set; get; } = false;

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
				_saveFileDialog.Filter = "Wavefront OBJ (*.obj)|*.obj|" +
										"COLLADA (*.dae)|*.dae";
				_saveFileDialog.Title = Resources.Model_Dialog_SelectExportFile;

				ChangeSkeletonCommand = new RelayCommand<string>(arg =>
				{
					SkeletonType.TryParse(arg, true, out SkeletonType result);
					skeletonType = result;
					RaisePropertyChanged("IsSkeletonHuman");
					RaisePropertyChanged("IsSkeletonHorse");
					RaisePropertyChanged("IsSkeletonOther");
					RaisePropertyChanged("IsSkeletonOtherAndRigged");
				});

				ChangeMaterialCommand = new RelayCommand<string>(arg =>
				{
					MaterialExportSetting.TryParse(arg, true, out MaterialExportSetting result);
					materialExport = result;
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
					_skeletons.Sort((left, right) =>
						{
							return StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name);
						});
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
				MessageBox.Show("You need to select a skeleton from the combo box.",
					"Error", MessageBoxButton.OK, MessageBoxImage.Stop);
				return;
			}

			Skeleton skeleton = null;
			if (ExportAsRigged)
			{
				switch (skeletonType)
				{
					case SkeletonType.Human:
						skeleton = _human_skeleton;
						break;
					case SkeletonType.Horse:
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
			if (materialExport == MaterialExportSetting.Export)
				option |= ModelExporter.ModelExportOption.ExportTextures;
			if (materialExport == MaterialExportSetting.ExportToSubFolder)
				option |= ModelExporter.ModelExportOption.ExportTexturesSubFolder;
			if (OnlyExportDiffuse)
				option |= ModelExporter.ModelExportOption.ExportDiffuseOnly;

			_saveFileDialog.FileName = Asset.Name;
			if (_saveFileDialog.ShowDialog().GetValueOrDefault(false))
			{
				var path = _saveFileDialog.FileName;

				MessengerInstance.Send(string.Format("Export {0} ...", Asset.Name), MainViewModel.StatusEvent);
				ModelExporter.ExportToFile(path, Asset, skeleton, option);
				MessengerInstance.Send(string.Format("{0} exported", Asset.Name), MainViewModel.StatusEvent);
			}
		}

		private enum SkeletonType
		{
			Human,
			Horse,
			Other
		}

		private enum MaterialExportSetting
		{
			None,
			Export,
			ExportToSubFolder
		}
	}
}