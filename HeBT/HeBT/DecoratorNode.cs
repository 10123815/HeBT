/*************************************************************

** Auth: ysd
** Date: 2016.1.8
** Desc: Decorator node
** Vers: v1.0

*************************************************************/

using System;

namespace HeBT
{

    /// <summary>
    /// Wrap a node with other behaviours.
    /// </summary>
    abstract public class DecoratorNode : NonLeafNode
    {
        private Node child;

        protected DecoratorNode (string name, Node child)
            : base(name)
        {
            Child = child;

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
                return child;
            }

            set
            {
                child = value;
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

        internal NegateNode(string name, Node child)
            :base(name, child)
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

}
