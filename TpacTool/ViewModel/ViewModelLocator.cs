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

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<LoadingViewModel>();
            //SimpleIoc.Default.Register<ModelPreviewViewModel>();
            SimpleIoc.Default.Register<TexturePreviewViewModel>();
			SimpleIoc.Default.Register<ModelViewModel>();
			SimpleIoc.Default.Register<TextureViewModel>();
			SimpleIoc.Default.Register<MaterialViewModel>();
			SimpleIoc.Default.Register<WpfPreviewViewModel>();
			// force init preview and panel
			//ViewModelBase unused = ModelPreview;
			ViewModelBase unused = WpfPreview;
			unused = TexturePreview;
			unused = Model;
			unused = Texture;
			unused = Material;
		}

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

		public LoadingViewModel Loading
		{
			get
			{
				return ServiceLocator.Current.GetInstance<LoadingViewModel>();
			}
		}

		/*public ModelPreviewViewModel ModelPreview
		{
			get
			{
				return ServiceLocator.Current.GetInstance<ModelPreviewViewModel>();
			}
		}*/

		public TexturePreviewViewModel TexturePreview
		{
			get
			{
				return ServiceLocator.Current.GetInstance<TexturePreviewViewModel>();
			}
		}

		public WpfPreviewViewModel WpfPreview
		{
			get
			{
				return ServiceLocator.Current.GetInstance<WpfPreviewViewModel>();
			}
		}

		public AbstractPreviewViewModel ModelPreview
		{
			get
			{
				return WpfPreview;
			}
		}

		public ModelViewModel Model
		{
			get
			{
				return ServiceLocator.Current.GetInstance<ModelViewModel>();
			}
		}

		public TextureViewModel Texture
		{
			get
			{
				return ServiceLocator.Current.GetInstance<TextureViewModel>();
			}
		}

		public MaterialViewModel Material
		{
			get
			{
				return ServiceLocator.Current.GetInstance<MaterialViewModel>();
			}
		}

		public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}