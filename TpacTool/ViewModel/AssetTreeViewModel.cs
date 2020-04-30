using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using LiteTreeView;
using TpacTool.Lib;

namespace TpacTool
{
	public class AssetTreeViewModel : ViewModelBase
	{
		public static readonly Guid AssetSelectedEvent = Guid.NewGuid();

		private bool _isPackageMode;

		public List<PackageTreeNode> PackageNodes { private set; get; }

		public List<AssetTreeNode> AssetNodes { private set; get; }

		public bool IsPackageMode
		{
			set
			{
				_isPackageMode = value;
				if (_isPackageMode)
					TreeItemSource = PackageNodes;
				else
					TreeItemSource = AssetNodes;
				RaisePropertyChanged("TreeItemSource");
			}
			get => _isPackageMode;
		}

		public override void Cleanup()
		{
			PackageNodes.Clear();
			AssetNodes.Clear();
			base.Cleanup();
		}

		public IList TreeItemSource { private set; get; }

		public AssetTreeViewModel(AssetManager manager, Guid typeGuid)
		{
			PackageNodes = new List<PackageTreeNode>();
			AssetNodes = new List<AssetTreeNode>();
			IsPackageMode = true;
			foreach (var package in manager.LoadedPackages)
			{
				var name = package.File.Name;
				bool hasAsset = false;
				var packageNode = new PackageTreeNode() { PackageName = name };
				foreach (var asset in package.Items)
				{
					if (asset.Type == typeGuid)
					{
						AssetTreeNode assetNode = null;
						if (typeGuid == Texture.TYPE_GUID)	
							assetNode = new TextureTreeNode() { Asset = asset };
						else
							assetNode = new AssetTreeNode() { Asset = asset };
						AssetNodes.Add(assetNode);
						packageNode.Assets.Add(assetNode);
						hasAsset = true;
					}
				}

				if (hasAsset)
				{
					packageNode.Assets.Sort((left, right) =>
						{
							return StringComparer.CurrentCultureIgnoreCase.Compare(left.Asset.Name, right.Asset.Name);
						});
					PackageNodes.Add(packageNode);
				}
			}

			PackageNodes.Sort((left, right) =>
				{
					return StringComparer.CurrentCultureIgnoreCase.Compare(left.PackageName, right.PackageName);
				});
			AssetNodes.Sort((left, right) =>
				{
					return StringComparer.CurrentCultureIgnoreCase.Compare(left.Asset.Name, right.Asset.Name);
				});
		}

		public void SelectAsset(AssetItem assetItem)
		{
			MessengerInstance.Send(assetItem, AssetSelectedEvent);
		}

		public class PackageTreeNode : IHaveChildren
		{
			public List<AssetTreeNode> Assets { private set; get; } = new List<AssetTreeNode>();

			public IEnumerable Children => Assets;

			public string PackageName { set; get; }

			public override string ToString()
			{
				return PackageName;
			}
		}

		public class AssetTreeNode : IHaveChildren
		{
			private static IEnumerable EMPTY_LIST = new List<object>();

			public IEnumerable Children => EMPTY_LIST;

			public AssetItem Asset { set; get; }

			public override string ToString()
			{
				return Asset.Name;
			}
		}

		public class TextureTreeNode : AssetTreeNode
		{
			public override string ToString()
			{
				if (!((Texture) Asset).HasPixelData)
					return Asset.Name + " (TileSets)";
				return base.ToString();
			}
		}
	}
}