using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpeningsMoeWpfClient
{
    class ConvertedMovieProvider : IMovieProvider
    {
        private readonly IMovieProvider decorated;
        private readonly IMovieConverter converter;
        private readonly DirectoryInfo targetDirectory;

        public ConvertedMovieProvider(IMovieProvider decorated, IMovieConverter converter, DirectoryInfo targetDirectory)
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
            var converted = Path.Combine(targetDirectory.Name, movie.ConvertedFileName);
            if(!File.Exists(converted))
                await converter.ConvertMovie(path, converted);
            return converted;
        }
    }
}