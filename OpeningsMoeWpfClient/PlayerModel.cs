using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Functional.Maybe;
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

        public async Task DownloadMoreMovies()
        {
            var movies = await MovieDownloader.FetchListOfMovies(webAppUri, converter);
            var shuffledMovies = CollectionUtils.Shuffled(movies.ToList(), random);
            CollectionUtils.ReplaceContentsWith(allMovies, shuffledMovies);

            var cachedMovie = GetCachedMovie(allMovies);
            if (cachedMovie != null)
            {
                allMovies.Add(cachedMovie);
                var indexOfLastElement = allMovies.Count - 1;
                // needs to be one less because RequestNextVideo() is called as the first thing
                CurrentMovieIndicator = indexOfLastElement - 1;
            }
        }

        private Movie GetCachedMovie(IEnumerable<Movie> allMovies)
        {
            var cachedMovies = MovieDownloader.FilterCachedMovies(allMovies).ToList();
            return CollectionUtils.Choice(cachedMovies, random).OrElseDefault();
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
    }
}
