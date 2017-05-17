using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpeningsMoeWpfClient
{
    class MovieDownloader : IMovieProvider
    {
        private readonly Uri openingsMoeUri;
        private readonly DirectoryInfo targetDirectory;
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
            var pathsToCachedFiles = new HashSet<string>(targetDirectory
                .EnumerateFiles("*.avi")
                .Select(file => Path.GetFileNameWithoutExtension(file.Name)));
            return (await Movies()).Where(movie => pathsToCachedFiles.Contains(Path.GetFileNameWithoutExtension(movie.LocalFileName)));
        }

        public async Task<string> GetPathToTheMovieFile(Movie movie)
        {
            string @where = Path.Combine(targetDirectory.Name, movie.LocalFileName);
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
            return where;
        }

        public MovieDownloader(Uri openingsMoeUri, DirectoryInfo targetDirectory)
        {
            this.openingsMoeUri = openingsMoeUri;
            this.targetDirectory = targetDirectory;
        }
    }
}
