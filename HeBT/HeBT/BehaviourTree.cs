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

        public BehaviourTree (NonLeafNode root)
        {
            m_root = root;
        }

        private NonLeafNode m_root;

        public NonLeafNode Root
        {
            get
            {
                return m_root;
            }
        }

        /// <summary>
        /// Receive hint from higher logic
        /// </summary>
        /// <param name="name">Node name to hint</param>
        /// <param name="hint">Hint type</param>
        public void ReceiveHint (string name, Common.HintType hint)
        {
            HandleHintInPathTo(m_root, name, hint);
        }

        public void CreationComplete ( )
        {
            Complete(m_root);
        }

        private void Complete (NonLeafNode root)
        {
            if (root is CompositeNode)
            {
                (root as CompositeNode).Complete();
            }
            int length = root.Children.Length;
            for (int i = 0; i < length; i++)
            {
                if (root.Children[i] is NonLeafNode)
                    Complete((root.Children[i] as NonLeafNode));
            }
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
