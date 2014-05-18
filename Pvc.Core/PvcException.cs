using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public class PvcException : Exception
    {
        public PvcException(string message, params string[] messageArgs)
            : base(string.Format(message, messageArgs))
        {
        }
    }
}
