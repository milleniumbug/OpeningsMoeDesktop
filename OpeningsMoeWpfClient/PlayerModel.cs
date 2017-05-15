using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpeningsMoeWpfClient
{
    class PlayerModel : INotifyPropertyChanged
    {
        private Random random;
        private readonly IMovieConverter converter;

        private Uri webAppUri;

        private ObservableCollection<Movie> allMovies = new ObservableCollection<Movie>();

        public int CurrentMovieIndicator { get; private set; } = 0;

        public IReadOnlyList<Movie> AllMovies => allMovies;

        public event PropertyChangedEventHandler PropertyChanged;

        private Task prefetchingTask;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void Shuffle<T>(IList<T> list, Random rand)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void ReplaceWith(IEnumerable<Movie> movies)
        {
            allMovies.Clear();
            foreach(var movie in movies)
            {
                allMovies.Add(movie);
            }
        }

        public async Task DownloadMoreMovies()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = webAppUri;
                using (var response = await httpClient.GetAsync("api/list.php"))
                {
                    var resultString = await response.Content.ReadAsStringAsync();
                    var movieList = JsonConvert.DeserializeObject<IEnumerable<MovieData>>(resultString)
                        .Select(data => new Movie(data, webAppUri, converter))
                        .ToList();
                    Shuffle(movieList, random);
                    ReplaceWith(movieList);
                }
            }
        }

        public async Task PrefetchNextMovie()
        {
            if(prefetchingTask == null)
                prefetchingTask = AllMovies[(CurrentMovieIndicator + 1) % AllMovies.Count].LoadVideoAndGetItsLocalPath();
            await prefetchingTask;
            prefetchingTask = null;
        }

        public async Task<Movie> RequestNextMovie()
        {
            ++CurrentMovieIndicator;
            CurrentMovieIndicator %= AllMovies.Count;
            var movie = AllMovies[CurrentMovieIndicator];
            if(prefetchingTask != null)
                await prefetchingTask;
            return movie;
        }

        public PlayerModel(Uri webAppUri, Random random, IMovieConverter converter)
        {
            this.webAppUri = webAppUri;
            this.random = random;
            this.converter = converter;
            Directory.CreateDirectory("Openings");
        }

        public static string TryLookupFfmpeg()
        {
            var enviromentPath = Environment.GetEnvironmentVariable("PATH");
            if(enviromentPath == null)
                return null;
            var paths = enviromentPath.Split(';');
            var exePath = paths
                .Select(x => Path.Combine(x, "ffmpeg.exe"))
                .FirstOrDefault(x => File.Exists(x));
            return string.IsNullOrWhiteSpace(exePath) == false ? exePath : null;
        }
    }
}
