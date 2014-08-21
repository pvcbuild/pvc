using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public class PvcDelayedPipe
    {
        public PvcDelayedPipe()
        {
            this.Stack = new List<Func<object>>();
            this.Pipe = new PvcPipe();
        }

        public PvcPipe Pipe { get; set; }

        public List<Func<object>> Stack { get; set; }

        public void ExecuteStack()
        {
            foreach (var item in this.Stack)
            {
                var result = item();
                if (result is PvcPipe)
                {
                    this.Pipe = (PvcPipe)result;
                }
                else if (result is PvcPlugins.PvcPlugin)
                {
                    this.Pipe.Pipe((PvcPlugins.PvcPlugin)result);
                }
                else if (result is Func<IEnumerable<PvcCore.PvcStream>, IEnumerable<PvcCore.PvcStream>>)
                {
                    this.Pipe.Pipe((Func<IEnumerable<PvcCore.PvcStream>, IEnumerable<PvcCore.PvcStream>>)result);
                }
            }
        }

        public static PvcDelayedPipe operator &(PvcDelayedPipe delayedPipe, PvcPlugins.PvcPlugin plugin)
        {
            delayedPipe.Stack.Add(() => plugin);
            return delayedPipe;
        }

        public static PvcDelayedPipe operator &(PvcDelayedPipe delayedPipe1, PvcDelayedPipe delayedPipe2)
        {
            delayedPipe1.Stack.AddRange(delayedPipe2.Stack);
            return delayedPipe1;
        }

        public static PvcDelayedPipe operator &(PvcDelayedPipe delayedPipe, Action action)
        {
            delayedPipe.Stack.Add(() => {
                action();
                return null;
            });

            return delayedPipe;
        }

        public static PvcDelayedPipe operator &(PvcDelayedPipe delayedPipe, Func<IEnumerable<PvcStream>, IEnumerable<PvcStream>> action)
        {
            delayedPipe.Stack.Add(() => action(delayedPipe.Pipe.streams));
            return delayedPipe;
        }
    }
}
