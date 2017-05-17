using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpeningsMoeWpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PlayerModel model;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task NextVideo()
        {
            var moviePath = await model.RequestNextMovie();
            OpeningPlayer.SetCurrentValue(
                MediaElement.SourceProperty,
                new Uri(
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        moviePath)));
            OpeningPlayer.Play();
        }

        private async void OnVideoFinishedPlaying(object sender, RoutedEventArgs e)
        {
            await NextVideo();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var ffmpegPath = FfmpegMovieConverter.TryLookupFfmpeg();
            if (ffmpegPath == null)
                throw new InvalidOperationException("FFMPEG NOT FOUND");
            model = await PlayerModel.Create(new Uri("http://openings.moe/"), new Random(), new FfmpegMovieConverter(ffmpegPath));
            DataContext = model;
            await NextVideo();
        }
    }
}
