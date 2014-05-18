using PvcCore;
using System;
using System.IO;

namespace Pvc.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var taskName = args.Length >= 1 ? args[0] : "default";

                var executor = new Executor(Path.Combine(Directory.GetCurrentDirectory(), "pvcfile.csx"));
                executor.Execute(taskName);
            }
            catch (PvcException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
