using System.Threading.Tasks;

namespace OpeningsMoeWpfClient
{
    interface IMovieConverter
    {
        Task<string> ConvertMovie(string sourcePath, string targetPath);
    }
}
