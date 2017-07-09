using CameraApp.Model;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace CameraApp
{
    public sealed partial class Library : Page
    {
        private static List<Photo> VideosList { get; set; }
        private static List<Photo> PhotosList { get; set; }

        public Library()
        {
            this.InitializeComponent();

            VideosList = new List<Photo>();
            PhotosList = new List<Photo>();
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

            getResources();

            // Set the resources
            photos.ItemsSource = PhotosList;
            videos.ItemsSource = VideosList;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //App.RootFrame.Navigate(typeof(MainPage));
            this.Frame.Navigate(typeof(MainPage));
        }

        private void getResources()
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
                        new Photo { Name = photo.Name, Path = new Uri(photo.Path) }
                    );
                }

                foreach (var video in videos)
                {
                    VideosList.Add(
                        new Photo { Name = video.Name, Path = new Uri(video.Path) }
                    );
                }
            }).Wait();
        }
    }
}
