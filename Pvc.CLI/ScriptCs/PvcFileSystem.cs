using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvc.CLI
{
    public class PvcFileSystem : ScriptCs.FileSystem
    {
        public override string PackagesFile
        {
            get
            {
                return "pvc-project.json";
            }
        }

        public override string PackagesFolder
        {
            get
            {
                return "pvc-packages";
            }
        }
    }
}
