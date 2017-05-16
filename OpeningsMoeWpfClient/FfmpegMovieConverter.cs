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
                    Arguments = $@"-i ""{sourcePath}"" -vcodec msmpeg4v2 -acodec libmp3lame -strict -2 ""{targetPath}""",
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            var watch = new Stopwatch();

            process.Exited += (sender, args) =>
            {
                if(process.ExitCode == 0)
                    tcs.SetResult(targetPath);
                else
                    tcs.SetException(new InvalidOperationException("SOMETHING WENT WRONG"));
                Console.WriteLine(watch.ElapsedMilliseconds);
                process.Dispose();
            };

            watch.Start();
            process.Start();
            process.PriorityClass = ProcessPriorityClass.BelowNormal;

            return tcs.Task;
        }

        public FfmpegMovieConverter(string ffmpegPath)
        {
            this.ffmpegPath = ffmpegPath;
        }
    }
}
