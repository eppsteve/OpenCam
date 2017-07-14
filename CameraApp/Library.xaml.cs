using CameraApp.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace CameraApp
{
    public sealed partial class Library : Page
    {
        private static ObservableCollection<GalleryItem> VideosList { get; set; }
        private static ObservableCollection<GalleryItem> PhotosList { get; set; }

        public Library()
        {
            this.InitializeComponent();

            VideosList = new ObservableCollection<GalleryItem>();
            PhotosList = new ObservableCollection<GalleryItem>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                    a.Handled = true;
                }
            };

            GetResources();

            // Set the resources
            photos.ItemsSource = PhotosList;
            videos.ItemsSource = VideosList;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //App.RootFrame.Navigate(typeof(MainPage));
            this.Frame.Navigate(typeof(MainPage));
        }

        private void GetResources()
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                var photos = await (await ApplicationData.Current.LocalFolder.GetFolderAsync("Photos")).GetFilesAsync();
                var videos = await (await ApplicationData.Current.LocalFolder.GetFolderAsync("Videos")).GetFilesAsync();
                //var photos = await (await KnownFolders.PicturesLibrary.GetFolderAsync("CameraApp")).GetFilesAsync();
                //var videos = await (await KnownFolders.VideosLibrary.GetFolderAsync("CameraApp")).GetFilesAsync();

                // Filter files
                foreach (var photo in photos)
                {
                    PhotosList.Add(
                        new GalleryItem { Name = photo.Name, Path = new Uri(photo.Path) }
                    );
                }

                foreach (var video in videos)
                {
                    VideosList.Add(
                        new GalleryItem { Name = video.Name, Path = new Uri(video.Path) }
                    );
                }
            }).Wait();
        }

        private async void Find_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder;
            string folder_str;
            var t = new FolderLauncherOptions();

            folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Photos");
            folder_str = "Photos\\";

            //get selected item
            foreach (var item in photos.SelectedItems)
            {
                var image = item as GalleryItem; //cast to photo
                var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, folder_str + image.Name); //get path
                StorageFile file = await StorageFile.GetFileFromPathAsync(path); //create storageFile from path
                t.ItemsToSelect.Add(file);                
            }

            await Launcher.LaunchFolderAsync(folder, t);
        }

        private void photos_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            GridView gridView = (GridView)sender;
            photosMenuFlyout.ShowAt(gridView, e.GetPosition(gridView));
            //var a = ((FrameworkElement)e.OriginalSource).DataContext;
        }

        private void videos_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            GridView gridView = (GridView)sender;
            videosMenuFlyout.ShowAt(gridView, e.GetPosition(gridView));
        }

        private async void FindVideo_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder;
            string folder_str;
            var t = new FolderLauncherOptions();

            folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Videos");
            folder_str = "Videos\\";

            //get selected item
            foreach (var item in videos.SelectedItems)
            {
                var video = item as GalleryItem;
                var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, folder_str + video.Name); //get path
                StorageFile file = await StorageFile.GetFileFromPathAsync(path); //create storageFile from path
                t.ItemsToSelect.Add(file);
            }

            await Launcher.LaunchFolderAsync(folder, t);
        }

        private async void DeletePhoto_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog showDialog = new MessageDialog("The selected photo will be deleted from your machine.", "Are you sure?");
            showDialog.Commands.Add(new UICommand("Yes")
            {
                Id = 0
            });
            showDialog.Commands.Add(new UICommand("No")
            {
                Id = 1
            });
            showDialog.DefaultCommandIndex = 0;
            showDialog.CancelCommandIndex = 1;
            var result = await showDialog.ShowAsync();

            if ((int)result.Id == 0)
            {
                var folder_str = "Photos\\";

                //get selected item
                foreach (var item in photos.SelectedItems)
                {
                    var photo = item as GalleryItem;
                    var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, folder_str + photo.Name); //get path
                    StorageFile file = await StorageFile.GetFileFromPathAsync(path); //create storageFile from path
                    await file.DeleteAsync();
                    PhotosList.Remove(photo);
                }
            }
        }

        private async void DeleteVideo_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog showDialog = new MessageDialog("The selected video will be deleted from your machine.", "Are you sure?");
            showDialog.Commands.Add(new UICommand("Yes")
            {
                Id = 0
            });
            showDialog.Commands.Add(new UICommand("No")
            {
                Id = 1
            });
            showDialog.DefaultCommandIndex = 0;
            showDialog.CancelCommandIndex = 1;
            var result = await showDialog.ShowAsync();
            if ((int)result.Id == 0)
            {
                var folder_str = "Videos\\";

                //get selected item
                foreach (var item in videos.SelectedItems)
                {
                    var video = item as GalleryItem;
                    var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, folder_str + video.Name); //get path
                    StorageFile file = await StorageFile.GetFileFromPathAsync(path); //create storageFile from path
                    await file.DeleteAsync();
                    VideosList.Remove(video);
                }
            }
        }
    }
}
