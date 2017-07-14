using System;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace CameraApp
{
    public sealed partial class MainPage : Page
    {
        private MediaCapture Capture { get; set; }
        private bool IsRecording { get; set; }
        private static DispatcherTimer Timer { get; set; }

        // Video capture settings
        private static MediaEncodingProfile Encoding { get; set; }
        private static string myEncoding = "";
        private static string myQuality = "";
        private static bool muted = false;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // load the resources.
            Capture = new MediaCapture();
            Init();
        }

        private async void Init()
        {
            //var x = await KnownFolders.PicturesLibrary.CreateFolderAsync("CameraApp", CreationCollisionOption.OpenIfExists);
            //var y = await KnownFolders.VideosLibrary.CreateFolderAsync("CameraApp", CreationCollisionOption.OpenIfExists);

            await Capture.InitializeAsync();

            captureElement.Source = Capture;
            await Capture.StartPreviewAsync();

            #region Error handling
            MediaCaptureFailedEventHandler handler = (sender, e) =>
            {
                System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Run(async () =>
                {
                    await new MessageDialog("There was an error capturing the video from camera.", "Error").ShowAsync();
                });
            };

            Capture.Failed += handler;
            #endregion

            #region Advanced topic; I will complete this region later
            try
            {
                // FaceTracker tracker = await FaceTracker.CreateAsync();
                // await tracker.ProcessNextFrameAsync(await capture.GetPreviewFrameAsync());
            }
            catch (Exception e)
            {
                await new MessageDialog(e.InnerException.Message, "Error").ShowAsync();
            }
            #endregion
        }

        private async void CapturePhoto()
        {
            if (!IsRecording)
            {
                var file = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("Photos",
                                  CreationCollisionOption.OpenIfExists)
                                  ).CreateFileAsync("Photo.jpg", CreationCollisionOption.GenerateUniqueName);

                //var file = await (await KnownFolders.PicturesLibrary.CreateFolderAsync("CameraApp", CreationCollisionOption.OpenIfExists))
                //                    .CreateFileAsync("Photo.jpg", CreationCollisionOption.GenerateUniqueName);

                await Capture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
            }
            else
            {
                await new MessageDialog("Application is currently recording your camera, please stop recording and try again.", "Recording").ShowAsync();
            }
        }

        private async void PlaySound()
        {
            MediaElement mysong = new MediaElement();
            StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile file = await folder.GetFileAsync("Camera-Shutter-Click.wav");
            var stream = await file.OpenAsync(FileAccessMode.Read);
            mysong.SetSource(stream, file.ContentType);
            mysong.Play();
        }

        private async void AlterRecording()
        {
            if (IsRecording)
            {
                // Stop recording
                await Capture.StopRecordAsync();
                videoIcon.Foreground = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 0, G = 0, B = 0 }); // Black
                IsRecording = false; // Not recording any more
            }
            else
            {
                Encoding = GetVideoEncoding(); // Get the current encoding selection.

                // Start recording
                var file = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("Videos",
                                    CreationCollisionOption.OpenIfExists)).CreateFileAsync(
                                        string.Format("Recording_{0}-{1}.{2}",
                                        myEncoding,
                                        myQuality,
                                        (myEncoding == "MP4") ? "mp4" : "wav"),
                                        CreationCollisionOption.GenerateUniqueName);

                await Capture.StartRecordToStorageFileAsync(Encoding, file);

                videoIcon.Foreground = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 255, G = 0, B = 0 }); // Red
                IsRecording = true; // Capturing the video stream.
            }
        }

        private MediaEncodingProfile GetVideoEncoding()
        {
            VideoEncodingQuality quality = VideoEncodingQuality.Auto;
            myQuality = "Auto";

            switch (videoQuality.SelectedIndex)
            {
                case 2:
                    quality = VideoEncodingQuality.HD1080p;
                    myQuality = "1080p";
                    break;
                case 3:
                    quality = VideoEncodingQuality.HD720p;
                    myQuality = "720p";
                    break;
                case 4:
                    quality = VideoEncodingQuality.Vga;
                    myQuality = "VGA";
                    break;
                default:
                    break;
            }

            myEncoding = (videoType == null || videoType.SelectedIndex == 0) ? "MP4" : "WMV";

            return (videoType == null || videoType.SelectedIndex == 0) ?
                MediaEncodingProfile.CreateMp4(quality) :
                MediaEncodingProfile.CreateWmv(quality);
        }

        private void InvertCamera()
        {
            if (captureElement.FlowDirection == FlowDirection.LeftToRight)
                captureElement.FlowDirection = FlowDirection.RightToLeft;
            else
                captureElement.FlowDirection = FlowDirection.LeftToRight;
        }

        private void MuteUnmute()
        {
            if (muted)
            {
                // Unmute
                Capture.AudioDeviceController.Muted = false;
                muted = false;
                muteIcon.Foreground = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 0, G = 0, B = 0 });
            }
            else
            {
                // Mute
                Capture.AudioDeviceController.Muted = true;
                muted = true;
                muteIcon.Foreground = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 255, G = 0, B = 0 });
            }
        }

        private async void CleanResources()
        {
            captureElement.Source = null;
            await Capture.StopPreviewAsync();

            if (IsRecording)
            {
                await Capture.StopRecordAsync();
            }
            Capture.Dispose(); // Dispose the resource
        }

        #region Events
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as AppBarButton).Name == "cameraIcon")
            {
                // Capture the image
                CapturePhoto();
                PlaySound();
            }
            else if ((sender as AppBarButton).Name == "videoIcon")
            {
                // Start recording
                AlterRecording();
                PlaySound();
            }
            else if ((sender as AppBarButton).Name == "rotateCameraIcon")
            {
                InvertCamera();
            }
            else if ((sender as AppBarButton).Name == "muteIcon")
            {
                MuteUnmute();
            }
            else if ((sender as AppBarButton).Name == "aboutIcon")
            {
                ShowAbout();
            }
            else if ((sender as AppBarButton).Name == "libraryIcon")
            {
                //App.RootFrame.Navigate(typeof(Library));
                this.Frame.Navigate(typeof(Library));

                CleanResources();
            }
        }

        private void ShowAbout()
        {
            var msg = new MessageDialog("OpenCam ver. 0.1\nsteve.alogaris@outlook.com", "About OpenCam...").ShowAsync();
        }

        private void videoType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Encoding = GetVideoEncoding(); // Get the encoding settings for current selection.
        }
#endregion
    }
}
