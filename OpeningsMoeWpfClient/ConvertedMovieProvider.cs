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
        public Task<IEnumerable<MovieDescription>> Movies()
        {
            return decorated.Movies();
        }

        /// <inheritdoc />
        public Task<IEnumerable<MovieDescription>> MoviesReady()
        {
            return decorated.MoviesReady();
        }

        /// <inheritdoc />
        public async Task<string> GetPathToTheMovieFile(MovieDescription movieDescription)
        {
            var path = await decorated.GetPathToTheMovieFile(movieDescription);
            var converted = Path.Combine(targetDirectory.Name, $"{Path.GetFileNameWithoutExtension(path)}.avi");
            if(!File.Exists(converted))
                await converter.ConvertMovie(path, converted);
            return converted;
        }
    }
}