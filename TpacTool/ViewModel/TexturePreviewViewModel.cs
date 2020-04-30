using System;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using TpacTool.Lib;
using TpacTool.Properties;

namespace TpacTool
{
	public class TexturePreviewViewModel : ViewModelBase
	{
		public static readonly Guid PriviewTextureEvent = Guid.NewGuid();

		public BitmapSource ImageSource { private set; get; }

		public string ImageText { private set; get; }

		public TexturePreviewViewModel()
		{
			if (!IsInDesignMode)
			{
				MessengerInstance.Register<AssetItem>(this, AssetTreeViewModel.AssetSelectedEvent, OnSelectAsset);
				MessengerInstance.Register<Texture>(this, PriviewTextureEvent, OnPreviewTexture);
				MessengerInstance.Register<object>(this, MainViewModel.CleanupEvent, unused =>
				{
					ImageText = String.Empty;
					ImageSource = null;
					RaisePropertyChanged("ImageSource");
					RaisePropertyChanged("ImageText");
				});
			}
		}

		private void OnSelectAsset(AssetItem assetItem)
		{
			var texture = assetItem as Texture;
			if (texture != null)
				OnPreviewTexture(texture);
		}

		private void OnPreviewTexture(Texture texture)
		{
			if (texture == null)
			{
				ImageText = String.Empty;
				ImageSource = null;
			}
			else if (texture.HasPixelData && texture.TexturePixels.IsLargeData)
			{
				ImageText = Resources.Preview_Msg_TextureSizeTooLarge;
				ImageSource = null;
			}
			else if (!texture.HasPixelData)
			{
				ImageText = Resources.Preview_Msg_TextureHasNoData;
				ImageSource = null;
			}
			else if (!texture.Format.IsSupported())
			{
				ImageText = string.Format(Resources.Preview_Msg_TextureFormatUnsupported, texture.Format.ToString());
				ImageSource = null;
			}
			else
			{
				ImageText = String.Empty;
				ShowTexturePreview(texture, TextureViewModel._clampMode, TextureViewModel._channelMode);
			}
			RaisePropertyChanged("ImageSource");
			RaisePropertyChanged("ImageText");
		}

		private void ShowTexturePreview(Texture texture, int clamp, int channel)
		{
			if (channel == 0)
			{
				switch (texture.Format.GetColorChannel())
				{
					case 4:
						channel = ResourceCache.CHANNEL_MODE_RGBA;
						break;
					case 3:
						channel = ResourceCache.CHANNEL_MODE_RGB;
						break;
					case 2:
						channel = ResourceCache.CHANNEL_MODE_RG;
						break;
					case 1:
						channel = ResourceCache.CHANNEL_MODE_R;
						break;
				}
			}

			switch (clamp)
			{
				case 1: // 2048
					clamp = 2048;
					break;
				case 2: // 1024
					clamp = 1024;
					break;
				case 3: // 512
					clamp = 512;
					break;
			}

			ImageSource = ResourceCache.GetImage(texture, clamp, channel);
		}
	}
}