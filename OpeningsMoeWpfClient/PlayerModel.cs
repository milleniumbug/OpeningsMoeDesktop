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

        private IEnumerator<Task<Movie>> movieSequenceEnumerator;

        public IReadOnlyList<Movie> AllMovies => allMovies;

        public event PropertyChangedEventHandler PropertyChanged;

        private Task prefetchingTask;
        private Movie currentMovie;

        private Movie CurrentMovie
        {
            get => currentMovie;
            set
            {
                if(currentMovie == value)
                    return;
                currentMovie = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentlyPlaying));
            }
        }

        public string CurrentlyPlaying => CurrentMovie != null
            ? $"{CurrentMovie.Title} - {CurrentMovie.Source}"
            : "Loading...";

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static Maybe<Movie> GetCachedMovie(IEnumerable<Movie> allMovies, Random random)
        {
            var cachedMovies = MovieDownloader.FilterCachedMovies(allMovies).ToList();
            return CollectionUtils.Choice(cachedMovies, random);
        }

        public async Task<string> RequestNextMovie()
        {
            if(CurrentMovie == null)
                movieSequenceEnumerator.MoveNext();
            CurrentMovie = await movieSequenceEnumerator.Current;
            movieSequenceEnumerator.MoveNext();
            return Path.Combine("Openings", CurrentMovie.ConvertedFileName);
        }

        private PlayerModel(Uri webAppUri, Random random, IMovieConverter converter)
        {
            this.webAppUri = webAppUri;
            this.random = random;
            this.converter = converter;
            Directory.CreateDirectory("Openings");
        }

        public static async Task<PlayerModel> Create(Uri webAppUri, Random random, IMovieConverter converter)
        {
            var playerModel = new PlayerModel(webAppUri, random, converter);
            var movies = await MovieDownloader.FetchListOfMovies(webAppUri, converter);
            var shuffledMovies = CollectionUtils.Shuffled(movies.ToList(), random);
            CollectionUtils.ReplaceContentsWith(playerModel.allMovies, shuffledMovies);

            var movieSequence = GetCachedMovie(playerModel.allMovies, playerModel.random)
                .ToEnumerable()
                .Concat(CollectionUtils.Cycle(playerModel.AllMovies));

            playerModel.movieSequenceEnumerator = movieSequence
                .Select(movie => MovieDownloader.DownloadMovie(webAppUri, movie, movie.LocalFileName, converter))
                .GetEnumerator();
            return playerModel;
        }
    }
}
