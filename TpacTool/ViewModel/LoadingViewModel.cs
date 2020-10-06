using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;

namespace TpacTool
{
	public class LoadingViewModel : ViewModelBase
	{
		public static readonly Guid LoadingProgressEvent = Guid.NewGuid();

		public static readonly Guid LoadingCancelledEvent = Guid.NewGuid();

		public string LoadingFileName { set; get; }

		public string ReadableLoadingProgress { set; get; }

		public int CurrentProgress { set; get; } = 0;

		public int MaxProgress { set; get; } = 100;

		public bool IsCompletedWithoutError { set; get; }

		public ICommand CancelLoadingCommand { set; get; }

		public LoadingViewModel()
		{
			if (IsInDesignMode)
			{
				LoadingFileName = "PLACEHOLDER";
				ReadableLoadingProgress = "PLACEHOLDER";
			}
			else
			{
				LoadingFileName = String.Empty;
				ReadableLoadingProgress = String.Empty;
				CancelLoadingCommand = new RelayCommand(CancelLoading);
				MessengerInstance.Register<ValueTuple<int, int, string>>(this, LoadingProgressEvent, message =>
					{
						ReadableLoadingProgress = message.Item1 + " / " + message.Item2;
						LoadingFileName = message.Item3;
						MaxProgress = message.Item2;
						CurrentProgress = message.Item1;
						RaisePropertyChanged("ReadableLoadingProgress");
						RaisePropertyChanged("LoadingFileName");
						RaisePropertyChanged("MaxProgress");
						RaisePropertyChanged("CurrentProgress");
					});
			}
		}

		private void CancelLoading()
		{
			MessengerInstance.Send<object>(null, LoadingCancelledEvent);
		}
	}
}