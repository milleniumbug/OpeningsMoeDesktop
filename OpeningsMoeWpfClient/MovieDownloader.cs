using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpeningsMoeWpfClient
{
    class ConvertedMovieProvider : IMovieProvider
    {
        private readonly IMovieProvider decorated;
        private readonly IMovieConverter converter;
        private readonly string targetDirectory;

        public ConvertedMovieProvider(IMovieProvider decorated, IMovieConverter converter, string targetDirectory)
        {
            this.decorated = decorated;
            this.converter = converter;
            this.targetDirectory = targetDirectory;
        }

        /// <inheritdoc />
        public Task<IEnumerable<Movie>> Movies()
        {
            return decorated.Movies();
        }

        /// <inheritdoc />
        public Task<IEnumerable<Movie>> MoviesReady()
        {
            return decorated.MoviesReady();
        }

        /// <inheritdoc />
        public async Task<string> GetPathToTheMovieFile(Movie movie)
        {
            var path = await decorated.GetPathToTheMovieFile(movie);
            var converted = Path.Combine(targetDirectory, movie.ConvertedFileName);
            if(!File.Exists(converted))
                await converter.ConvertMovie(path, converted);
            return converted;
        }
    }

    class MovieDownloader : IMovieProvider
    {
        private readonly Uri openingsMoeUri;
        private readonly string targetDirectory;
        private IEnumerable<Movie> movies;

        /// <inheritdoc />
        public async Task<IEnumerable<Movie>> Movies()
        {
            if(movies == null)
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = openingsMoeUri;
                    using (var response = await httpClient.GetAsync("api/list.php"))
                    {
                        var resultString = await response.Content.ReadAsStringAsync();
                        movies = JsonConvert.DeserializeObject<IEnumerable<MovieData>>(resultString)
                            .Select(data => new Movie(data))
                            .ToList();
                    }
                }
            }
            return movies;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Movie>> MoviesReady()
        {
            var pathsToCachedFiles = new HashSet<string>(new DirectoryInfo(targetDirectory)
                .EnumerateFiles("*.avi")
                .Select(file => Path.GetFileNameWithoutExtension(file.Name)));
            return (await Movies()).Where(movie => pathsToCachedFiles.Contains(Path.GetFileNameWithoutExtension(movie.LocalFileName)));
        }

        public async Task<string> GetPathToTheMovieFile(Movie movie)
        {
            string @where = Path.Combine(targetDirectory, movie.LocalFileName);
            var sourceExists = File.Exists(@where);
            if(!sourceExists)
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = openingsMoeUri;
                    using (var file = File.OpenWrite(@where))
                    using (var stream = await httpClient.GetStreamAsync($"video/{movie.RemoteFileName}"))
                    {
                        await stream.CopyToAsync(file);
                    }
                }
            }
            return Path.Combine(targetDirectory, movie.LocalFileName);
        }

        public MovieDownloader(Uri openingsMoeUri, string targetDirectory)
        {
            this.openingsMoeUri = openingsMoeUri;
            this.targetDirectory = targetDirectory;
        }
    }
}
