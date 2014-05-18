using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public abstract class PvcPlugin
    {
        public abstract IEnumerable<PvcStream> Execute(IEnumerable<PvcStream> inputStreams);
    }
}
