using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpeningsMoeWpfClient
{
    interface IMovieProvider
    {
        // Get the movie descriptions that this movie provider can provide path to
        Task<IEnumerable<MovieDescription>> Movies();

        // Get the movie descriptions which this movie provider can immediately provide access to
        // (that is, the returned Task becomes available very soon)
        Task<IEnumerable<MovieDescription>> MoviesReady();

        Task<string> GetPathToTheMovieFile(MovieDescription movieDescription);
    }
}
