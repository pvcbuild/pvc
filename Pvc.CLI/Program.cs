using PvcCore;
using System;
using System.IO;
using Edokan.KaiZen.Colors;
using System.Text.RegularExpressions;
using System.Linq;

namespace Pvc.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            PvcConsole.Configure();
            Directory.SetCurrentDirectory(@"C:\Users\stirno\Documents\GitHub");

            var task = new ArgumentHandler().Parse(args);
            task();
        }
    }
}
