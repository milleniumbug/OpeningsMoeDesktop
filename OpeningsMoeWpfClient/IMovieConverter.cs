using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpeningsMoeWpfClient
{
    interface IMovieConverter
    {
        Task<string> ConvertMovie(string sourcePath, string targetPath);
    }
}
