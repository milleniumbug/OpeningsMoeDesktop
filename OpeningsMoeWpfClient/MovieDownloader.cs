﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpeningsMoeWpfClient
{
    static class MovieDownloader
    {
        public static async Task<IEnumerable<Movie>> FetchListOfMovies(Uri uri, IMovieConverter converter)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = uri;
                using (var response = await httpClient.GetAsync("api/list.php"))
                {
                    var resultString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IEnumerable<MovieData>>(resultString)
                        .Select(data => new Movie(data, uri, converter))
                        .ToList();
                }
            }
        }

        public static async Task DownloadMovie(Uri webAppUri, Movie movie, string where)
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

        public static IObservable<Movie> DownloadMovies(ICollection<Movie> movies)
        {
            return Observable.Create<Movie>(async (IObserver<Movie> o) =>
            {
                foreach(var movie in CollectionUtils.Cycle(movies))
                {
                    await movie.LoadVideoAndGetItsLocalPath();
                    o.OnNext(movie);
                }
                return () => {};
            });
        }
    }
}
