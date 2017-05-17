using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpeningsMoeWpfClient
{
    interface IMovieProvider
    {
        // Get the movie descriptions that this movie provider can provide path to
        Task<IEnumerable<Movie>> Movies();

        // Get the movie descriptions which this movie provider can immediately provide access to
        // (that is, the returned Task becomes available very soon)
        Task<IEnumerable<Movie>> MoviesReady();

        Task<string> GetPathToTheMovieFile(Movie movie);
    }
}
