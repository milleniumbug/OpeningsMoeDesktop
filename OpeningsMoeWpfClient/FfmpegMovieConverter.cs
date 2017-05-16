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

        private Task<int> LaunchProcess(Func<Process> factory, Action<Process> postLaunchConfiguration)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = factory();

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            postLaunchConfiguration(process);

            return tcs.Task;
        }

        public async Task<string> ConvertMovie(string sourcePath, string targetPath)
        {
            int exitCode = await LaunchProcess(() => new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    FileName = ffmpegPath,
                    Arguments =
                        $@"-i ""{sourcePath}"" -vcodec msmpeg4v2 -acodec libmp3lame -strict -2 ""{targetPath}""",
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            }, process =>
            {
                process.PriorityClass = ProcessPriorityClass.BelowNormal;
            });

            if(exitCode == 0)
                return targetPath;
            throw new InvalidOperationException("SOMETHING WENT WRONG");
        }

        public FfmpegMovieConverter(string ffmpegPath)
        {
            this.ffmpegPath = ffmpegPath;
        }

        public static string TryLookupFfmpeg()
        {
            var enviromentPath = Environment.GetEnvironmentVariable("PATH");
            if (enviromentPath == null)
                return null;
            var paths = enviromentPath.Split(';');
            var exePath = paths
                .Select(x => Path.Combine(x, "ffmpeg.exe"))
                .FirstOrDefault(File.Exists);
            return string.IsNullOrWhiteSpace(exePath) == false ? exePath : null;
        }
    }
}
