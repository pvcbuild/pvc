using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public class PvcCircularDependencyException : PvcException
    {
        public List<string> CircularPath;

        public PvcCircularDependencyException(List<string> circularPath, string message, params string[] messageArgs)
            : base(message, messageArgs)
        {
            CircularPath = circularPath;
        }

        public PvcCircularDependencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
