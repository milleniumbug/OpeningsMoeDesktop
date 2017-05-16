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

        private int currentMovieIndicator;
        public int CurrentMovieIndicator
        {
            get => currentMovieIndicator;
            private set
            {
                if(currentMovieIndicator == value)
                    return;

                currentMovieIndicator = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentMovie));
                OnPropertyChanged(nameof(CurrentlyPlaying));
            }
        }

        public IReadOnlyList<Movie> AllMovies => allMovies;

        public event PropertyChangedEventHandler PropertyChanged;

        private Task prefetchingTask;

        public Movie CurrentMovie => AllMovies.Count > 0 ? AllMovies[CurrentMovieIndicator] : null;

        public string CurrentlyPlaying => CurrentMovie != null
            ? $"{CurrentMovie.Title} - {CurrentMovie.Source}"
            : "Loading...";

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

                    var cachedMovie = GetCachedMovie(allMovies);
                    if(cachedMovie != null)
                    {
                        allMovies.Add(cachedMovie);
                        var indexOfLastElement = allMovies.Count - 1;
                        // needs to be one less because RequestNextVideo() is called as the first thing
                        CurrentMovieIndicator = indexOfLastElement - 1;
                    }
                }
            }
        }

        private Movie GetCachedMovie(IEnumerable<Movie> allMovies)
        {
            var cachedFiles = new DirectoryInfo("Openings")
                .EnumerateFiles("*.avi")
                .Select(file => Path.GetFileNameWithoutExtension(file.Name))
                .ToList();
            if(cachedFiles.Count == 0)
                return null;
            var randomChoice = cachedFiles[random.Next(cachedFiles.Count)];
            return allMovies.Single(movie => movie.FileName.Contains(randomChoice));
        }

        public async Task PrefetchNextMovie()
        {
            if(prefetchingTask == null || prefetchingTask.IsCompleted)
                prefetchingTask = AllMovies[(CurrentMovieIndicator + 1) % AllMovies.Count].LoadVideoAndGetItsLocalPath();
            await prefetchingTask;
            prefetchingTask = null;
        }

        public async Task<Movie> RequestNextMovie()
        {
            CurrentMovieIndicator = (CurrentMovieIndicator+1) % AllMovies.Count;
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
