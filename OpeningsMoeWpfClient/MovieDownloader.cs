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
        private IEnumerable<MovieDescription> movies;

        private string SafeFilePathFor(string filename)
        {
            return filename
                .Replace(':', '_')
                .Replace('\\', '_')
                .Replace('*', '_')
                .Replace('?', '_')
                .Replace('"', '_')
                .Replace('<', '_')
                .Replace('>', '_')
                .Replace('|', '_');
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MovieDescription>> Movies()
        {
            if(movies == null)
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = openingsMoeUri;
                    using (var response = await httpClient.GetAsync("api/list.php"))
                    {
                        var resultString = await response.Content.ReadAsStringAsync();
                        movies = JsonConvert.DeserializeObject<IEnumerable<MovieJsonObject>>(resultString)
                            .Select(data => new MovieDescription(data))
                            .ToList();
                    }
                }
            }
            return movies;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MovieDescription>> MoviesReady()
        {
            var pathsToCachedFiles = new HashSet<string>(targetDirectory
                .EnumerateFiles("*.webm")
                .Select(file => Path.GetFileNameWithoutExtension(file.Name)));
            return (await Movies()).Where(movie => pathsToCachedFiles.Contains(Path.GetFileNameWithoutExtension(SafeFilePathFor(movie.RemoteFileName))));
        }

        public async Task<string> GetPathToTheMovieFile(MovieDescription movieDescription)
        {
            string @where = Path.Combine(targetDirectory.Name, SafeFilePathFor(movieDescription.RemoteFileName));
            var temporaryPath = Path.Combine(Directory.GetCurrentDirectory(),
                $"downloaded{Path.GetExtension(movieDescription.RemoteFileName)}");
            var sourceExists = File.Exists(@where);
            if(!sourceExists)
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = openingsMoeUri;
                    using (var file = File.Create(temporaryPath))
                    using (var stream = await httpClient.GetStreamAsync($"video/{movieDescription.RemoteFileName}"))
                    {
                        await stream.CopyToAsync(file);
                    }
                }
                File.Move(temporaryPath, where);
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
