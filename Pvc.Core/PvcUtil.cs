using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public static class PvcUtil
    {
        public static PvcStream StringToStream(string data, string streamName)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.Write(data);
            writer.Flush();

            return new PvcStream(stream).As(streamName);
        }

        public static string StreamToTempFile(PvcStream stream)
        {
            // be sure to start from beginning
            stream.Position = 0;

            var tmpFileName = Path.GetTempFileName();
            File.WriteAllText(tmpFileName, new StreamReader(stream).ReadToEnd());

            return tmpFileName;
        }

        public static Tuple<Stream, Stream> StreamProcessExecution(string processPath, string workingDirectory, params string[] args)
        {
            var startInfo = new ProcessStartInfo(processPath, string.Join(" ", args));
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.ErrorDialog = false;
            startInfo.WorkingDirectory = workingDirectory;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;

            var process = Process.Start(startInfo);
            return new Tuple<Stream, Stream>(process.StandardOutput.BaseStream, process.StandardError.BaseStream);
        }

        public static string PathRelativeToCurrentDirectory(string absolutePath)
        {
            string currentDir = Environment.CurrentDirectory;
            DirectoryInfo directory = new DirectoryInfo(currentDir);
            FileInfo file = new FileInfo(absolutePath);

            string fullDirectory = directory.FullName;
            string fullFile = file.FullName;

            if (fullFile.StartsWith(fullDirectory))
            {
                // The +1 is to avoid the directory separator
                return fullFile.Substring(fullDirectory.Length + 1);
            }

            // could not generate a relative path, let the abs path through?
            return absolutePath;
        }

        public static string FindBinaryInPath(params string[] binaries)
        {
            var searchBinaries = new List<string>();
            foreach (var binary in binaries)
            {
                if (File.Exists(binary))
                    return Path.GetFullPath(binary);

                if (Path.GetDirectoryName(binary) == string.Empty)
                    searchBinaries.Add(binary);
            }

            var searchDirectories = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';');

            // Add chocolatey bin (in case the users PATH is busted)
            searchDirectories.Concat(new[] { Path.Combine(Environment.GetEnvironmentVariable("ChocolateyInstall"), "bin") });

            foreach (string test in searchDirectories)
            {
                string path = test.Trim();

                foreach (var searchBinary in searchBinaries)
                {
                    var binary = Environment.ExpandEnvironmentVariables(searchBinary);
                    var binaryRelPath = Path.Combine(path, binary);
                    if (!String.IsNullOrEmpty(path) && File.Exists(binaryRelPath))
                        return Path.GetFullPath(binaryRelPath);
                }
            }

            if (binaries.Length == 1)
                throw new FileNotFoundException("Unable to find requested binary in PATH: " + binaries[0]);
            else
                throw new FileNotFoundException("Unable to find any of the requested binaries in PATH: " + string.Join(", ", binaries));
        }
    }
}
