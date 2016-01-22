/*************************************************************

** Auth: ysd
** Date: 2015.12.23
** Desc: Hinted-Excution Behaviour Trees
** Vers: v1.0

*************************************************************/

using System.Collections.Generic;

namespace HeBT
{

    /// <summary>
    /// Client need to extend this class to add some Create** method to create behaviour nodes
    /// </summary>
    abstract public class TreeNodeFactor
    {

        public TreeNodeFactor ( )
        {
            m_nameSpace = new HashSet<string>();
        }

        private HashSet<string> m_nameSpace;

        protected void CheckName (string name)
        {
            if (m_nameSpace.Contains(name))
            {
                string msg = string.Format("Node name {0} has already existed.", name);
                throw new System.Exception(msg);
            }
            // available name
            m_nameSpace.Add(name);
        }

        /// <summary>
        /// Create a sequence node.
        /// </summary>
        /// <param name="name">Node name</param>
        /// <param name="length">Possible number of children</param>
        /// <param name="children">Child nodes of this sub-tree, may be null</param>
        public SequenceNode CreateSequence (string name, byte length, Blackboard bb = null, params Node[] children)
        {
            CheckName(name);

            if (children == null)
            {
                return new SequenceNode(name, length);
            }
            else
            {
                SequenceNode seq = new SequenceNode(name, length);
                seq.AddChildren(children);
                return seq;
            }
        }

        /// <summary>
        /// Create a selector node.
        /// </summary>
        /// <param name="name">Node name</param>
        /// <param name="length">Possible number of children</param>
        /// <param name="children">Child nodes of this sub-tree, may be null</param>
        public SelectorNode CreateSelector (string name, byte length, Blackboard bb = null, params Node[] children)
        {
            CheckName(name);

            SelectorNode sel = new SelectorNode(name, length);
            if (children != null)
            {
                sel.AddChildren(children);
            }
            if (bb != null)
            {
                sel.blackboard = bb;
            }

            return sel;

        }

        /// <summary>
        /// Create a selector node. This selector will return its first child in next frame if it's successful
        /// </summary>
        /// <param name="name">Node name</param>
        /// <param name="length">Possible number of children</param>
        /// <param name="children">Child nodes of this sub-tree, may be null</param>
        public ReSelectorNode CreateReSelector (string name, byte length, params Node[] children)
        {
            CheckName(name);

            ReSelectorNode resel = new ReSelectorNode(name, length);
            if (children != null)
            {
                resel.AddChildren(children);
            }
            return resel;

        }

        /// <summary>
        /// Create a selector node. This selector will reorder its children when it receive hint.
        /// </summary>
        /// <param name="name">Node name</param>
        /// <param name="length">Possible number of children</param>
        /// <param name="children">Child nodes of this sub-tree, may be null</param>
        public HintedSelectorNode CreateHintedSelector (string name, byte length, params Node[] children)
        {
            CheckName(name);

            HintedSelectorNode hintedSel = new HintedSelectorNode(name, length);
            if (children != null)
            {
                hintedSel.AddChildren(children);
            }
            return hintedSel;
        }

        /// <summary>
        /// Create a selector node. This selector will reorder its children when it receive hint. And it will return to its first child in next frame when it;s successful
        /// </summary>
        /// <param name="name">Node name</param>
        /// <param name="length">Possible number of children</param>
        /// <param name="children">Child nodes of this sub-tree, may be null</param>
        public ReHintedSelectorNode CreateReHintedSelector (string name, byte length, params Node[] children)
        {
            CheckName(name);

            ReHintedSelectorNode reHintedSel = new ReHintedSelectorNode(name, length);
            if (children != null)
            {
                reHintedSel.AddChildren(children);
            }
            return reHintedSel;
        }

        /// <summary>
        /// Create a parallel node. Its each child runs and only runs one time. When all children have completed running, it return success if all children are successful.
        /// </summary>
        /// <param name="name">Node name</param>
        /// <param name="length">Possible number of children</param>
        /// <param name="children">Child nodes of this sub-tree, may be null</param>
        public ParallelNodeOnceAll CreateParallel (string name, byte length, params Node[] children)
        {
            CheckName(name);

            ParallelNodeOnceAll oncePar = new ParallelNodeOnceAll(name, length);
            if (children != null)
            {
                oncePar.AddChildren(children);
            }
            return oncePar;
        }

        /// <summary>
        /// Create a parallel node. Failed once one child is failed, success when a given number children is successful 
        /// </summary>
        /// <param name="name">Node name</param>
        /// <param name="length">Possible number of children</param>
        /// <param name="limitation">Max number of successful child</param>
        /// <param name="children">Child nodes of this sub-tree, may be null</param>
        public ParallelSeqNode CreateParallelSequence (string name, byte length, byte limitation = byte.MaxValue, params Node[] children)
        {
            CheckName(name);

            ParallelSeqNode parSeq = new ParallelSeqNode(name, length, limitation);
            if (children != null)
            {
                parSeq.AddChildren(children);
            }
            return parSeq;
        }

        /// <summary>
        /// Create a parallel node. Success once one child is successful, failed when a given number children is failed 
        /// </summary>
        /// <param name="name">Node name</param>
        /// <param name="length">Possible number of children</param>
        /// <param name="limitation">Max number of failed child</param>
        /// <param name="children">Child nodes of this sub-tree, may be null</param>
        public ParallelSelNode CreateParallelSelector (string name, byte length, byte limitation = byte.MaxValue, params Node[] children)
        {
            CheckName(name);

            ParallelSelNode parSel= new ParallelSelNode(name, length, limitation);
            if (children != null)
            {
                parSel.AddChildren(children);
            }
            return parSel;
        }

        /// <summary>
        /// Create a negator to negate the child's executing result
        /// </summary>
        public NegateNode CreateNegateDecorator (string name, Node child)
        {
            CheckName(name);
            return new NegateNode(name, child);
        }

        /// <summary>
        /// Create a loop node to repeat running its child times
        /// </summary>
        public LoopNode CreateLoopDecorator (string name, Node child, byte loopTimes)
        {
            CheckName(name);
            return new LoopNode(name, child, loopTimes);
        }

        public InfiniteLoopNode CreateInfiniteDecorator (string name, Node child)
        {
            CheckName(name);
            return new InfiniteLoopNode(name, child);
        }

        /// <summary>
        /// Create a wrapper to de something else after running a child.
        /// </summary>
        /// <param name="wrapper">Wrapping method</param>
        public WrapperNode CreateWrapperDecorator (string name, Node child, WrapperNode.DelWrapper wrapper)
        {
            CheckName(name);
            return new WrapperNode(name, child, wrapper);
        }

        public BehaviourTree CreateBT (NonLeafNode root)
        {
            return new BehaviourTree(root);
        }

        /// <summary>
        /// Completing the creation and allocate IDs
        /// </summary>
        public void Complete (NonLeafNode root, byte id = 1)
        {
            // bfs
            Queue<Node> nodeQueue = new Queue<Node>();
            byte l = 1;
            nodeQueue.Enqueue(root);
            while (l > 0)
            {
                Node node = nodeQueue.Dequeue();
                l -= 1;

                // allocate ids via bread-first-search
                node.ID = id;
                id += 1;

                if (node is NonLeafNode)
                {
                    NonLeafNode nonLeafNode = (node as NonLeafNode);
                    nonLeafNode.blackboard.Parent = FindParentBlackboard(nonLeafNode);
                    int childNumber = nonLeafNode.Children.Length;
                    for (int i = 0; i < childNumber; i++)
                    {
                        nodeQueue.Enqueue(nonLeafNode.Children[i]);
                    }

                    if (nonLeafNode is CompositeNode)
                    {
                        (nonLeafNode as CompositeNode).Complete();
                    }
                }
            }
        }

        /// <summary>
        /// Search blackboard at parent
        /// </summary>
        private Blackboard FindParentBlackboard (NonLeafNode nonLeafNode)
        {
            if (nonLeafNode.Parent != null)
            {
                if (nonLeafNode.Parent.blackboard != null)
                {
                    return nonLeafNode.Parent.blackboard;
                }
                return FindParentBlackboard(nonLeafNode.Parent);
            }

            return null;
        }

    }

    public class BehaviourTree
    {

        internal BehaviourTree (NonLeafNode root)
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
