/*************************************************************

** Auth: ysd
** Date: 2016.1.8
** Desc: Decorator node
** Vers: v1.0

*************************************************************/

namespace HeBT
{

    /// <summary>
    /// Wrap a node with other behaviours.
    /// </summary>
    abstract public class DecoratorNode : NonLeafNode
    {
        private Node m_child;

        protected DecoratorNode (string name, Node child)
            : base(name)
        {
            m_child = child;
            m_child.Parent = this;

            if (m_children == null)
            {
                m_children = new Node[1];
                m_children[0] = child;
            }
        }

        public Node Child
        {
            get
            {
                return m_child;
            }

            set
            {
                m_child = value;
                if (m_children == null)
                {
                    m_children = new Node[1];
                    m_children[0] = value;
                }
            }
        }
    }

    /// <summary>
    /// Successful if child is failed; failed if child is successful
    /// </summary>
    public class NegateNode : DecoratorNode
    {

        internal NegateNode (string name, Node child)
            : base(name, child)
        { }

        public override Common.NodeExecuteState Execute ( )
        {
            Common.NodeExecuteState state = Child.Execute();

            if (state == Common.NodeExecuteState.g_kSuccess)
            {
                return Common.NodeExecuteState.g_kFailure;
            }

            if (state == Common.NodeExecuteState.g_kFailure)
            {
                return Common.NodeExecuteState.g_kSuccess;
            }

            return Common.NodeExecuteState.g_kRunning;
        }

    }

    /// <summary>
    /// Loop node keep executing its child until limit is reached.
    /// Return fail if child is failed.
    /// </summary>
    public class LoopNode : DecoratorNode
    {

        /// <summary>
        /// repeat for loopCount times
        /// </summary>
        private byte m_loopTimes;

        public byte LoopTimes
        {
            get
            {
                return m_loopTimes;
            }

            set
            {
                m_loopTimes = value;
            }
        }

        private byte m_currentCount;

        internal LoopNode (string name, Node child, byte loopCount)
            : base(name, child)
        {
            m_loopTimes = loopCount;
        }

        public override Common.NodeExecuteState Execute ( )
        {
            Common.NodeExecuteState state = Child.Execute();
            if (state == Common.NodeExecuteState.g_kFailure)
            {
                m_currentCount = 0;
                return state;
            }
            else if (++m_currentCount > m_loopTimes)
            {
                if (state == Common.NodeExecuteState.g_kSuccess)
                {
                    m_currentCount = 0;
                }
                return state;
            }
            return Common.NodeExecuteState.g_kRunning;
        }
    }

    /// <summary>
    /// Loop until its child is failed
    /// </summary>
    public class InfiniteLoopNode : DecoratorNode
    {

        internal InfiniteLoopNode (string name, Node child)
            : base(name, child)
        { }

        public override Common.NodeExecuteState Execute ( )
        {
            Common.NodeExecuteState state = Child.Execute();
            if (state == Common.NodeExecuteState.g_kFailure)
            {
                return Common.NodeExecuteState.g_kFailure;
            }
            return Common.NodeExecuteState.g_kRunning;
        }

    }

    /// <summary>
    /// Do something else after child's executing.
    /// </summary>
    public class WrapperNode : DecoratorNode
    {

        internal WrapperNode (string name, Node child, DelWrapper wrapper)
            : base(name, child)
        {
            m_wrapper = wrapper;
        }

        /// <summary>
        /// Do something else
        /// </summary>
        public delegate Common.NodeExecuteState DelWrapper ( );
        private DelWrapper m_wrapper;

        public override Common.NodeExecuteState Execute ( )
        {
            Child.Execute();
            return m_wrapper();
        }
    }

    /// <summary>
    /// Some External condition.
    /// </summary>
    abstract public class PreconditionNode : DecoratorNode
    {
        internal PreconditionNode (string name, Node child)
            : base(name, child)
        { }

        public override Common.NodeExecuteState Execute ( )
        {
            if (PreCheck())
                return Child.Execute();
            else
                return Common.NodeExecuteState.g_kFailure;
        }

        abstract public bool PreCheck ( );
    }

    abstract public class ConditionJumpNode : PreconditionNode
    {
        private string m_jumpName;

        internal ConditionJumpNode (string name, Node child, string jumpName)
            : base(name, child)
        {
            m_jumpName = jumpName;
        }

        public override Common.NodeExecuteState Execute ( )
        {
            // if true, jump to the desired node
            if (PreCheck())
            {
                // set current child of the parent of the desired node pointing the desired node


                return Common.NodeExecuteState.g_kSuccess;
            }

            return Child.Execute();
        }

        /// <summary>
        /// If the desired node is in parent
        /// </summary>
        private bool SearchInParent (NonLeafNode parent, string name)
        {
            return false;
        }

        /// <summary>
        /// If the desired node is in children
        /// </summary>
        private bool SearchInChild (NonLeafNode parent, string name)
        {
            return false;
        }

    }

}
