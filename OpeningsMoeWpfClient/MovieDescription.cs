namespace OpeningsMoeWpfClient
{
    class MovieDescription
    {
        public string RemoteFileName => movieJsonObject.File;

        public string Title => movieJsonObject.Title;

        public string Source => movieJsonObject.Source;

        private readonly MovieJsonObject movieJsonObject;

        public MovieDescription(MovieJsonObject movieJsonObject)
        {
            this.movieJsonObject = movieJsonObject;
        }
    }
}
