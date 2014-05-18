using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public class PvcException : Exception
    {
        public PvcException(string message, params string[] messageArgs)
            : base(string.Format(message, messageArgs))
        {
            PreserveStackTrace(this);
        }

        public PvcException(Exception ex)
            : base(ex.Message, ex)
        {
            PreserveStackTrace(this);
        }

        public PvcException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <remarks>
        /// Credit to MvcContrib.TestHelper.AssertionException for PreserveStackTrace
        /// </remarks>
        private static void PreserveStackTrace(Exception e)
        {
            var ctx = new StreamingContext(StreamingContextStates.CrossAppDomain);
            var mgr = new ObjectManager(null, ctx);
            var si = new SerializationInfo(e.GetType(), new FormatterConverter());

            e.GetObjectData(si, ctx);
            mgr.RegisterObject(e, 1, si);
            mgr.DoFixups();
        }
    }
}
