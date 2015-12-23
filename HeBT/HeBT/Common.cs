/*************************************************************

** Auth: ysd
** Date: 2015.12.23
** Desc: Constant define
** Vers: v1.0

*************************************************************/

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
