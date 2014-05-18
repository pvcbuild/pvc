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
    }
}
