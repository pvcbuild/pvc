using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edokan.KaiZen.Colors;
using System.Threading;

namespace PvcCore
{
    public class PvcConsole
    {
        public static Dictionary<int, string> CurrentTaskByThread = new Dictionary<int, string>();

        public static string ThreadTask
        {
            get
            {
                if (CurrentTaskByThread.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                    return CurrentTaskByThread[Thread.CurrentThread.ManagedThreadId];

                return null;
            }

            set
            {
                if (value == null)
                    CurrentTaskByThread.Remove(Thread.CurrentThread.ManagedThreadId);
                else
                    CurrentTaskByThread[Thread.CurrentThread.ManagedThreadId] = value;
            }
        }

        public static string Tag = "[".Grey() + "pvc".Cyan() + "]".Grey();

        public static string TaskOutputTag
        {
            get
            {
                return ThreadTask != null ? "[".Grey() + ThreadTask.Magenta() + "] ".Grey() : "";
            }
        }

        public static void Configure()
        {
            Edokan.KaiZen.Colors.EscapeSequencer.Install();
        }
    }
}
