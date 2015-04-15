using ScriptCs.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvc.CLI
{
    public class PvcConsoleLogger : ILog
    {
        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters)
        {
            if (messageFunc != null)
            {
                var message = string.Format(CultureInfo.InvariantCulture, messageFunc(), formatParameters);
                Console.WriteLine(message);
            }

            return true;
        }
    }

    public class PvcNullLogger : ILog
    {
        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters)
        {
            return true;
        }
    }
}
