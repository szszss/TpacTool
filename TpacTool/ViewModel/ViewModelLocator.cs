/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:TpacTool"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using System;
using System.IO;
using System.Windows;
using Assimp.Unmanaged;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

namespace TpacTool
{
	/// <summary>
	/// This class contains static references to all the view models in the
	/// application and provides an entry point for the bindings.
	/// </summary>
	public class ViewModelLocator
	{
		/// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

			////if (ViewModelBase.IsInDesignModeStatic)
			////{
			////    // Create design time view services and models
			////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
			////}
			////else
			////{
			////    // Create run time view services and models
			////    SimpleIoc.Default.Register<IDataService, DataService>();
			////}

			AssimpLibrary.Instance.Resolver.SetProbingPaths32(
				Path.Combine(Environment.CurrentDirectory, "bin\\win-x86\\native\\"));
			AssimpLibrary.Instance.Resolver.SetProbingPaths64(
				Path.Combine(Environment.CurrentDirectory, "bin\\win-x64\\native\\"));
			AssimpLibrary.Instance.Resolver.SetFallbackLibraryNames32("assimp.dll");
			AssimpLibrary.Instance.Resolver.SetFallbackLibraryNames64("assimp.dll");

			SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<LoadingViewModel>();
			SimpleIoc.Default.Register<ModelViewModel>();
			SimpleIoc.Default.Register<TextureViewModel>();
			SimpleIoc.Default.Register<MaterialViewModel>();
			SimpleIoc.Default.Register<AnimationViewModel>();
			SimpleIoc.Default.Register<OglPreviewViewModel>();
			
			ViewModelBase unused = null;
			// force init preview and panel
			unused = OglPreview;
			unused = Model;
			unused = Texture;
			unused = Material;
			unused = Animation;
		}

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public LoadingViewModel Loading => ServiceLocator.Current.GetInstance<LoadingViewModel>();
		
		public OglPreviewViewModel OglPreview => ServiceLocator.Current.GetInstance<OglPreviewViewModel>();

		public ModelViewModel Model => ServiceLocator.Current.GetInstance<ModelViewModel>();

		public TextureViewModel Texture => ServiceLocator.Current.GetInstance<TextureViewModel>();

		public MaterialViewModel Material => ServiceLocator.Current.GetInstance<MaterialViewModel>();

		public AnimationViewModel Animation => ServiceLocator.Current.GetInstance<AnimationViewModel>();

		public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}