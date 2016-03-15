/*************************************************************

** Auth: ysd
** Date: 2015.12.23
** Desc: Constant define.
** Vers: v1.0

*************************************************************/

namespace HeBT
{
    static public class Common
    {

        public enum NodeExecuteState : byte
        {
            kSuccess = 0,
            kFailure = 1,
            kRunning = 2,
            kInvalid = 3
        }
        
        public enum HintType : byte
        {
            kPositive = 0,
            kNeutral = 1,
            kNegative = 2
        }
        

    }
}
