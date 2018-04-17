using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Mpv.WPF;

namespace OpeningsMoeWpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Mpv.WPF.MpvPlayer openingPlayer;

        private PlayerModel model;

        public MainWindow()
        {
            InitializeComponent();
            openingPlayer = new Mpv.WPF.MpvPlayer("lib\\mpv-1.dll");
            openingPlayer.MediaUnloaded += OnVideoFinishedPlaying;
            PlayerContainer.Children.Add(openingPlayer);
        }

        private async Task NextVideo()
        {
            var moviePath = await model.RequestNextMovie();
            openingPlayer.Load(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    moviePath),
                Mpv.WPF.LoadMethod.Replace);
            openingPlayer.Resume();
        }

        private async void OnVideoFinishedPlaying(object sender, EventArgs e)
        {
            await NextVideo();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var targetDirectory = new DirectoryInfo("Openings");
            model = await PlayerModel.Create(
                new Random(),
                new MovieDownloader(
                    new Uri("http://openings.moe/"),
                    targetDirectory),
                targetDirectory);
            model.PropertyChanged += OnVolumeChanged;
            DataContext = model;
            await NextVideo();
        }

        private void OnVolumeChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Volume")
            {
                openingPlayer.Volume = (int)(model.Volume * 100);
            }
        }

        private void OnClosed(object sender, EventArgs e)
        {
            model.Dispose();
        }

        private void OnPlayerClick(object sender, RoutedEventArgs e)
        {
            if(openingPlayer.IsPlaying)
                openingPlayer.Pause();
            else
                openingPlayer.Resume();
        }
    }
}
