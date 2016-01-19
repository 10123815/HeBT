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

        public DecoratorNode (string name, Node child)
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

        public NegateNode(string name, Node child)
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
        private byte m_loopCount;

        public byte LoopCount
        {
            get
            {
                return m_loopCount;
            }

            set
            {
                m_loopCount = value;
            }
        }

        private byte m_currentCount;

        public LoopNode (string name, Node child, byte loopCount)
            : base(name, child)
        {
            m_loopCount = loopCount;
        }

        public override Common.NodeExecuteState Execute ( )
        {
            Common.NodeExecuteState state = Child.Execute();
            if (state == Common.NodeExecuteState.g_kFailure)
            {
                m_currentCount = 0;
                return state;
            }
            else if (++m_currentCount > m_loopCount)
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
    /// Do something else after child's executing.
    /// </summary>
    public class WrapperNode : DecoratorNode
    {

        public WrapperNode (string name, Node child)
            : base(name, child)
        {

        }

        /// <summary>
        /// Do something
        /// </summary>
        /// <returns></returns>
        public virtual Common.NodeExecuteState Wrapper ( )
        {
            return Common.NodeExecuteState.g_kSuccess;
        }

        public override Common.NodeExecuteState Execute ( )
        {
            Child.Execute();
            return Wrapper();
        }
    }

    /// <summary>
    /// Some External condition.
    /// </summary>
    abstract public class PreconditionNode : DecoratorNode
    {
        public PreconditionNode (string name, Node child)
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
