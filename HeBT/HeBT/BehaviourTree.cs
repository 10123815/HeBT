/*************************************************************

** Auth: ysd
** Date: 2015.12.23
** Desc: Hinted-Excution Behaviour Trees
** Vers: v1.0

*************************************************************/

namespace HeBT
{
    public class BehaviourTree
    {

        public void ReceiveHint(string name, Common.HintType hint)
        {
            
        }

        public delegate void DelHandleTargetChild(int childIndex);
        public delegate void DelHandleTargetChildWithHint(int childIndex, Common.HintType hint);

        private bool HandleInPathTo(NonLeafNode root, string nodeName, DelHandleTargetChild handler)
        {
            if (root.NodeName == nodeName)
            {
                return true;
            }
            else
            {
                Node[] children = root.Children;
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i] is NonLeafNode)
                    {
                        bool has = HandleInPathTo((NonLeafNode)children[i], nodeName, handler);
                        // we find it at this child
                        handler(i);
                        return true;
                    }
                    else if (children[i].NodeName == nodeName)
                    {
                        handler(i);
                        return true;
                    }
                }
                return false;
            }
        }

    }
}
