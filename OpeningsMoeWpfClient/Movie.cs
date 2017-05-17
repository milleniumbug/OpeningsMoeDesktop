using System;
using System.IO;
using System.Threading.Tasks;

namespace OpeningsMoeWpfClient
{
    class Movie
    {
        public async Task<string> LoadVideoAndGetItsLocalPath()
        {
            var localPath = Path.Combine("Openings", LocalFileName);
            var newLocalPath = Path.Combine("Openings", ConvertedFileName);
            var sourceExists = File.Exists(localPath);
            var adaptedExists = File.Exists(newLocalPath);
            if(!sourceExists)
            {
                await MovieDownloader.DownloadMovie(webAppUri, this, localPath);
            }
            if(!adaptedExists)
            {
                await converter.ConvertMovie(localPath, newLocalPath);
            }
            return newLocalPath;
        }

        public string LocalFileName => RemoteFileName
            .Replace(':', '_')
            .Replace('\\', '_')
            .Replace('*', '_')
            .Replace('?', '_')
            .Replace('"', '_')
            .Replace('<', '_')
            .Replace('>', '_')
            .Replace('|', '_');

        private string ConvertedFileName => $"{Path.GetFileNameWithoutExtension(LocalFileName)}.avi";

        public string RemoteFileName => movieData.file;

        public string Title => movieData.title;

        public string Source => movieData.source;

        private readonly MovieData movieData;

        private readonly IMovieConverter converter;

        private readonly Uri webAppUri;

        public Movie(MovieData movieData, Uri webAppUri, IMovieConverter converter)
        {
            this.movieData = movieData;
            this.webAppUri = webAppUri;
            this.converter = converter;
        }
    }
}
