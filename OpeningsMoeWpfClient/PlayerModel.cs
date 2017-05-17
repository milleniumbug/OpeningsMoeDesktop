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
    class PlayerModel : INotifyPropertyChanged, IDisposable
    {
        private Random random;
        private readonly IMovieProvider movieProvider;

        private ObservableCollection<Movie> allMovies = new ObservableCollection<Movie>();

        private IEnumerator<Task<KeyValuePair<Movie, string>>> movieSequenceEnumerator;

        public IReadOnlyList<Movie> AllMovies => allMovies;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public async Task<string> RequestNextMovie()
        {
            if(CurrentMovie == null)
                movieSequenceEnumerator.MoveNext();
            var moviePathPair = await movieSequenceEnumerator.Current;
            CurrentMovie = moviePathPair.Key;
            movieSequenceEnumerator.MoveNext();
            return moviePathPair.Value;
        }

        private PlayerModel(Random random, IMovieProvider movieProvider, DirectoryInfo targetDirectory)
        {
            this.random = random;
            this.movieProvider = movieProvider;
            targetDirectory.Create();
        }

        public static async Task<PlayerModel> Create(Random random, IMovieProvider movieProvider, DirectoryInfo targetDirectory)
        {
            var playerModel = new PlayerModel(random, movieProvider, targetDirectory);
            var movies = await movieProvider.Movies();
            var shuffledMovies = CollectionUtils.Shuffled(movies.ToList(), random);
            CollectionUtils.ReplaceContentsWith(playerModel.allMovies, shuffledMovies);

            var moviesReady = (await movieProvider.MoviesReady()).ToList();
            var movieSequence = CollectionUtils.Choice(moviesReady, random)
                .ToEnumerable()
                .Concat(CollectionUtils.Cycle(playerModel.AllMovies));

            playerModel.movieSequenceEnumerator = movieSequence
                .Select(async movie => new KeyValuePair<Movie, string>(movie, await movieProvider.GetPathToTheMovieFile(movie)))
                .GetEnumerator();
            return playerModel;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            movieSequenceEnumerator?.Dispose();
        }
    }
}
