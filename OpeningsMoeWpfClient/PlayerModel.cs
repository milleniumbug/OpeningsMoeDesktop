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

        private ObservableCollection<MovieDescription> allMovies = new ObservableCollection<MovieDescription>();

        private IEnumerator<Task<KeyValuePair<MovieDescription, string>>> movieSequenceEnumerator;

        public IReadOnlyList<MovieDescription> AllMovies => allMovies;

        public event PropertyChangedEventHandler PropertyChanged;

        private MovieDescription currentMovieDescription;

        private MovieDescription CurrentMovieDescription
        {
            get => currentMovieDescription;
            set
            {
                if(currentMovieDescription == value)
                    return;
                currentMovieDescription = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentlyPlaying));
            }
        }

        public string CurrentlyPlaying => CurrentMovieDescription != null
            ? $"{CurrentMovieDescription.Title} - {CurrentMovieDescription.Source}"
            : "Loading...";

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<string> RequestNextMovie()
        {
            if(CurrentMovieDescription == null)
                movieSequenceEnumerator.MoveNext();
            var moviePathPair = await movieSequenceEnumerator.Current;
            CurrentMovieDescription = moviePathPair.Key;
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
                .Select(async movie => new KeyValuePair<MovieDescription, string>(movie, await movieProvider.GetPathToTheMovieFile(movie)))
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
