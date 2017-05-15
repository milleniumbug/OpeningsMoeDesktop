using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpeningsMoeWpfClient
{
    class Movie
    {
        public async Task<string> LoadVideoAndGetItsLocalPath()
        {
            var localPath = Path.Combine("Openings", FileName);
            var newLocalPath = Path.Combine("Openings", NewFileName);
            var sourceExists = File.Exists(localPath);
            var adaptedExists = File.Exists(newLocalPath);
            if(!sourceExists)
            {
                using(var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = webAppUri;
                    using(var file = File.OpenWrite(localPath))
                    using(var stream = await httpClient.GetStreamAsync($"video/{FileName}"))
                    {
                        await stream.CopyToAsync(file);
                    }
                }
            }
            if(!adaptedExists)
            {
                await converter.ConvertMovie(localPath, newLocalPath);
            }
            return newLocalPath;
        }

        private string NewFileName => $"{Path.GetFileNameWithoutExtension(FileName)}.mp4";

        private string FileName => movieData.file;

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
