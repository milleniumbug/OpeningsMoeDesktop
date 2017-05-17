using System.IO;
using System.Threading.Tasks;

namespace OpeningsMoeWpfClient
{
    class Movie
    {
        public string LocalFileName => RemoteFileName
            .Replace(':', '_')
            .Replace('\\', '_')
            .Replace('*', '_')
            .Replace('?', '_')
            .Replace('"', '_')
            .Replace('<', '_')
            .Replace('>', '_')
            .Replace('|', '_');

        public string ConvertedFileName => $"{Path.GetFileNameWithoutExtension(LocalFileName)}.avi";

        public string RemoteFileName => movieData.file;

        public string Title => movieData.title;

        public string Source => movieData.source;

        private readonly MovieData movieData;

        public Movie(MovieData movieData)
        {
            this.movieData = movieData;
        }
    }
}
