using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Gu.Wpf.Media;

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
            var ffmpegPath = PlayerModel.TryLookupFfmpeg();
            if(ffmpegPath == null)
                throw new InvalidOperationException("FFMPEG NOT FOUND");
            model = new PlayerModel(new Uri("http://openings.moe/"), new Random(), new FfmpegMovieConverter(ffmpegPath));
            DataContext = model;
            InitializeComponent();
        }

        private async Task NextVideo()
        {
            var movie = await model.RequestNextMovie();
            OpeningPlayer.SetCurrentValue(
                MediaElementWrapper.SourceProperty,
                new Uri(
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        await movie.LoadVideoAndGetItsLocalPath())));
            OpeningPlayer.Play();
            await model.PrefetchNextMovie();
        }

        private async void OnVideoFinishedPlaying(object sender, RoutedEventArgs e)
        {
            await NextVideo();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await model.DownloadMoreMovies();
            await NextVideo();
        }
    }
}
