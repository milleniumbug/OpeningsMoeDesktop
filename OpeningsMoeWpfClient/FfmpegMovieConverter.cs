using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpeningsMoeWpfClient
{
    class FfmpegMovieConverter : IMovieConverter
    {
        private string ffmpegPath;

        public Task<string> ConvertMovie(string sourcePath, string targetPath)
        {
            var tcs = new TaskCompletionSource<string>();

            var process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    FileName = ffmpegPath,
                    Arguments = $@"-i ""{sourcePath}"" -vcodec h264 -acodec aac -strict -2 ""{targetPath}""",
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                if(process.ExitCode == 0)
                    tcs.SetResult(targetPath);
                else
                    tcs.SetException(new InvalidOperationException("SOMETHING WENT WRONG"));
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }

        public FfmpegMovieConverter(string ffmpegPath)
        {
            this.ffmpegPath = ffmpegPath;
        }
    }
}
