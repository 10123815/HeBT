using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeBT
{
    static public class Common
    {

        public enum NodeExecuteState : byte
        {
            g_kSuccess = 0,
            g_kFailure = 1,
            g_kRunning = 2,
            g_kInvalid = 3
        }

        public enum HintType : byte
        {
            g_kPositive = 0,
            g_kNeutral = 1,
            g_kNegative = 2
        }

    }
}
