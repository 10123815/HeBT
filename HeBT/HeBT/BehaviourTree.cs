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

        private NonLeafNode m_root;

        public NonLeafNode Root
        {
            set
            {
                m_root = value;
            }
        }

        public void Run ( ) { m_root.Execute(); }

        /// <summary>
        /// Receive hint from higher logic
        /// </summary>
        /// <param name="name">Node name to hint</param>
        /// <param name="hint">Hint type</param>
        public void ReceiveHint (string name, Common.HintType hint)
        {
            HandleHintInPathTo(m_root, name, hint);
        }

        private bool HandleHintInPathTo (NonLeafNode root, string nodeName, Common.HintType hint)
        {
            if (root.NodeName == nodeName)
            {
                return true;
            }
            else
            {
                Node[] children = root.Children;
                for (byte i = 0; i < children.Length; i++)
                {
                    if (children[i] is NonLeafNode)
                    {
                        bool has = HandleHintInPathTo((NonLeafNode)children[i], nodeName, hint);
                        // we find it at this hinted selector's child
                        if (has && root is HintedSelectorNode)
                        {
                            (root as HintedSelectorNode).Hinted(i, hint);
                        }
                        return true;
                    }
                    else if (children[i].NodeName == nodeName)
                    {
                        // we find it at this hinted selector's child
                        if (root is HintedSelectorNode)
                        {
                            (root as HintedSelectorNode).Hinted(i, hint);
                        }
                        return true;
                    }
                }
                return false;
            }
        }

    }
}
