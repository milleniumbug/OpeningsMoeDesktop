using System.IO;
using System.Threading.Tasks;

namespace OpeningsMoeWpfClient
{
    class MovieDescription
    {
        public string RemoteFileName => movieJsonObject.file;

        public string Title => movieJsonObject.title;

        public string Source => movieJsonObject.source;

        private readonly MovieJsonObject movieJsonObject;

        public MovieDescription(MovieJsonObject movieJsonObject)
        {
            this.movieJsonObject = movieJsonObject;
        }
    }
}
