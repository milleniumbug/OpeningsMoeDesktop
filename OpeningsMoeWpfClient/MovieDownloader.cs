using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpeningsMoeWpfClient
{
    static class MovieDownloaderGizmo
    {
        public static async Task<IEnumerable<Movie>> FetchListOfMovies(Uri uri)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = uri;
                using (var response = await httpClient.GetAsync("api/list.php"))
                {
                    var resultString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IEnumerable<MovieData>>(resultString)
                        .Select(data => new Movie(data))
                        .ToList();
                }
            }
        }

        public static async Task<Movie> DownloadLowMovie(Uri webAppUri, Movie movie, string where)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = webAppUri;
                using (var file = File.OpenWrite(where))
                using (var stream = await httpClient.GetStreamAsync($"video/{movie.RemoteFileName}"))
                {
                    await stream.CopyToAsync(file);
                }
            }
            return movie;
        }

        public static async Task<Movie> DownloadLowMovieIfExists(Uri webAppUri, Movie movie, string where)
        {
            var sourceExists = File.Exists(where);
            if(!sourceExists)
            {
                await DownloadLowMovie(webAppUri, movie, where);
            }
            return movie;
        }

        public static async Task<Movie> DownloadMovie(Uri webAppUri, Movie movie, string where, IMovieConverter converter)
        {
            var localPath = Path.Combine("Openings", movie.LocalFileName);
            var newLocalPath = Path.Combine("Openings", movie.ConvertedFileName);

            await MovieDownloaderGizmo.DownloadLowMovieIfExists(webAppUri, movie, localPath);

            var adaptedExists = File.Exists(newLocalPath);
            if (!adaptedExists)
            {
                await converter.ConvertMovie(localPath, newLocalPath);
            }
            return movie;
        }

        // given the sequence of movies, return the ones that can be played immediately,
        // without waiting for a download
        public static IEnumerable<Movie> FilterCachedMovies(IEnumerable<Movie> movies)
        {
            var pathsToCachedFiles = new HashSet<string>(new DirectoryInfo("Openings")
                .EnumerateFiles("*.avi")
                .Select(file => Path.GetFileNameWithoutExtension(file.Name)));
            return movies.Where(movie => pathsToCachedFiles.Contains(Path.GetFileNameWithoutExtension(movie.LocalFileName)));
        }
    }

    class ConvertedMovieProvider : IMovieProvider
    {
        private readonly IMovieProvider decorated;
        private readonly IMovieConverter converter;

        public ConvertedMovieProvider(IMovieProvider decorated, IMovieConverter converter)
        {
            this.decorated = decorated;
            this.converter = converter;
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
            return await converter.ConvertMovie(path, Path.Combine("Openings", movie.ConvertedFileName));
        }
    }

    class MovieDownloader : IMovieProvider
    {
        private readonly Uri openingsMoeUri;
        private readonly string targetDirectory;
        private Task<IEnumerable<Movie>> movies;

        /// <inheritdoc />
        public async Task<IEnumerable<Movie>> Movies()
        {
            if(movies == null)
                movies = MovieDownloaderGizmo.FetchListOfMovies(openingsMoeUri);
            return await movies;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Movie>> MoviesReady()
        {
            return MovieDownloaderGizmo.FilterCachedMovies(await Movies());
        }

        public async Task<string> GetPathToTheMovieFile(Movie movie)
        {
            await MovieDownloaderGizmo.DownloadLowMovieIfExists(openingsMoeUri, movie,
                Path.Combine(targetDirectory, movie.LocalFileName));
            return movie.LocalFileName;
        }

        public MovieDownloader(Uri openingsMoeUri, string targetDirectory)
        {
            this.openingsMoeUri = openingsMoeUri;
            this.targetDirectory = targetDirectory;
        }
    }
}
