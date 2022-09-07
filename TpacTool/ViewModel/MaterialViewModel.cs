using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Ookii.Dialogs.Wpf;
using TpacTool.IO;
using TpacTool.Lib;
using TpacTool.Properties;

namespace TpacTool
{
	public class MaterialViewModel : ViewModelBase
	{
		internal static readonly string[] _preferredFormatItems = new[]
		{
			Resources.Material_Export_PreferredFormat_Any,
			Resources.Material_Export_PreferredFormat_Png,
			Resources.Material_Export_PreferredFormat_Dds
		};

		private VistaFolderBrowserDialog _saveFilesDialog;

		private TextureSlotItem[] _emptyTextureSlotItems;
		private bool _showEmptySlots = false;
		private Texture _selectedTexture;
		private int _selectedTextureIndex = -1;

		public string[] ClampItems { private set; get; } = TextureViewModel._clampItems;

		public string[] ChannelItems { private set; get; } = TextureViewModel._channelItems;

		public string[] PreferredFormatItems => _preferredFormatItems;

		public int PreferredFormat { set; get; } = 0;

		public Texture SelectedTexture
		{
			private set
			{
				_selectedTexture = value;
				MessengerInstance.Send(_selectedTexture, OglPreviewViewModel.PreviewTextureEvent);
			}
			get => _selectedTexture;
		}

		public Material Asset { private set; get; }

		public Shader Shader
		{
			get
			{
				if (Asset != null && Asset.Shader.TryGetItem(out var shader))
					return shader;
				return null;
			}
		}

		public int ClampMode
		{
			set
			{
				TextureViewModel._clampMode = value;
				MessengerInstance.Send(_selectedTexture, OglPreviewViewModel.PreviewTextureEvent);
			}
			get => TextureViewModel._clampMode;
		}

		public int ChannelMode
		{
			set
			{
				TextureViewModel._channelMode = value;
				MessengerInstance.Send(_selectedTexture, OglPreviewViewModel.PreviewTextureEvent);
			}
			get => TextureViewModel._channelMode;
		}

		public bool IsExportable { set; get; } = false;

		public ICommand ExportCommand { private set; get; }

		public bool ShowEmptySlots
		{
			set
			{
				_showEmptySlots = value;
				RefreshTextures();
			}
			get => _showEmptySlots;
		}

		public ObservableCollection<TextureSlotItem> Textures { private set; get; }

		public int SelectedTextureIndex
		{
			set
			{
				_selectedTextureIndex = value;
				if (_selectedTextureIndex >= 0 && _selectedTextureIndex < Textures.Count)
				{
					SelectedTexture = Textures[_selectedTextureIndex]._item;
				}
				else
				{
					SelectedTexture = null;
				}
			}
			get => _selectedTextureIndex;
		}

		public MaterialViewModel()
		{
			Textures = new ObservableCollection<TextureSlotItem>();

			if (IsInDesignMode)
			{
			}
			else
			{
				_emptyTextureSlotItems = new TextureSlotItem[10];
				for (var i = 0; i < _emptyTextureSlotItems.Length; i++)
				{
					_emptyTextureSlotItems[i] = new TextureSlotItem(i, null);
				}

				_saveFilesDialog = new VistaFolderBrowserDialog();
				_saveFilesDialog.Description = Resources.Material_Dialog_SelectExportDir;
				_saveFilesDialog.UseDescriptionForTitle = true;
				_saveFilesDialog.ShowNewFolderButton = true;

				MessengerInstance.Register<AssetItem>(this, AssetTreeViewModel.AssetSelectedEvent, OnSelectAsset);
				MessengerInstance.Register<object>(this, MainViewModel.CleanupEvent, unused =>
					{
						_selectedTexture = null;
						Asset = null;
						Textures.Clear();
						RaisePropertyChanged("SelectedTexture");
						RaisePropertyChanged("Asset");
					});

				ExportCommand = new RelayCommand(OnExport);
			}
		}

		private void OnSelectAsset(AssetItem assetItem)
		{
			var material = assetItem as Material;
			if (material != null)
			{
				Asset = material;
				IsExportable = false;
				//SelectedTexture = null;
				RefreshTextures();
				RaisePropertyChanged("Asset");
				RaisePropertyChanged("IsExportable");
				RaisePropertyChanged("Shader");
			}
		}

		private void RefreshTextures()
		{
			Textures.Clear();

			int i;
			if (ShowEmptySlots)
			{
				for (i = 0; i < _emptyTextureSlotItems.Length; i++)
				{
					Textures.Add(_emptyTextureSlotItems[i]);
				}
			}

			if (Asset != null)
			{
				foreach (var pair in Asset.Textures)
				{
					if (!pair.Value.IsEmpty() && pair.Value.TryGetItem(out var tex))
					{
						var item = new TextureSlotItem(pair.Key, tex);
						if (Textures.Count > pair.Key)
							Textures[pair.Key] = item;
						else
							Textures.Add(item);
					}
				}
			}

			i = 0;
			foreach (var pair in Asset.Textures)
			{
				if (!pair.Value.IsEmpty() && pair.Value.TryGetItem(out var tex) && tex.HasPixelData)
				{
					IsExportable = true;
					if (ShowEmptySlots)
						SelectedTextureIndex = pair.Key;
					else
						SelectedTextureIndex = i;
					break;
				}
				i++;
			}

			RaisePropertyChanged("SelectedTexture");
			RaisePropertyChanged("SelectedTextureIndex");
		}

		private void OnExport()
		{
			if (Asset == null || !IsExportable)
				return;
			if (_saveFilesDialog.ShowDialog().GetValueOrDefault(false))
			{
				var path = _saveFilesDialog.SelectedPath;
				MaterialExporter.MaterialExportOption option = 0;
				switch (PreferredFormat)
				{
					case 1:
						option |= MaterialExporter.MaterialExportOption.PreferPng;
						break;
					case 2:
						option |= MaterialExporter.MaterialExportOption.PreferDds;
						break;
				}
				MessengerInstance.Send(string.Format("Export {0} ...", Asset.Name), MainViewModel.StatusEvent);
				MaterialExporter.ExportToFolder(path, Asset, option);
				MessengerInstance.Send(string.Format("{0} exported", Asset.Name), MainViewModel.StatusEvent);
			}
		}

		public class TextureSlotItem
		{
			public int Slot { set; get; }
			public string Texture { set; get; }
			internal Texture _item;

			public TextureSlotItem(int slot, Texture item)
			{
				Slot = slot;
				_item = item;
				Texture = String.Empty;
				if (_item != null)
				{
					Texture = _item.Name;
					if (!_item.HasPixelData)
						Texture = Texture + " (TileSets)";
				}
			}
		}
	}
}