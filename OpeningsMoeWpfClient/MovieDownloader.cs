using System;
using System.Collections.Generic;
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
