using System;

namespace HeBT
{

    abstract public class Node
    {

        private string m_nodeName;

        public string NodeName
        {
            get
            {
                return m_nodeName;
            }

            set
            {
                m_nodeName = value;
            }
        }

        public Node ( )
        {
            NodeName = "default";
        }

        public Node (string name)
        {
            m_nodeName = name;
        }

        virtual protected void OnInitialize ( ) { }

        abstract public Common.NodeExecuteState Execute ( );

    }

    #region Decorator

    /// <summary>
    /// Wrap a behaviour node with other behaviour
    /// </summary>
    abstract public class DecoratorNode : Node
    {
        private Node child;

        public DecoratorNode ( )
        { }

        public DecoratorNode (Node child)
        {
            Child = child;
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
            }
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

        public LoopNode (Node child, byte loopCount)
            : base(child)
        {
            m_loopCount = loopCount;
        }

        public override Common.NodeExecuteState Execute ( )
        {
            Common.NodeExecuteState result = Child.Execute();
            if (result == Common.NodeExecuteState.g_kFailure)
            {
                m_currentCount = 0;
                return result;
            }
            else if (++m_currentCount > m_loopCount)
            {
                m_currentCount = 0;
                return Common.NodeExecuteState.g_kSuccess;
            }
            return Common.NodeExecuteState.g_kRunning;
        }
    }

    /// <summary>
    /// Do something else after child's executing
    /// </summary>
    public class WrapperNode : DecoratorNode
    {

        public WrapperNode (Node child)
            : base(child)
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
    /// Some External condition
    /// </summary>
    abstract public class PreconditionNode : DecoratorNode
    {
        public PreconditionNode (Node child)
            : base(child)
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

    #endregion Decorator

    #region Composite

    abstract public class CompositeNode : Node
    {
        protected byte m_currentChildIndex;

        protected Node[] m_children;

        private byte m_count;

        public byte Count
        {
            get
            {
                return m_count;
            }
        }

        public CompositeNode ( ) { }

        public CompositeNode (string name, byte length)
            : base(name)
        {
            m_children = new Node[length];
            m_currentChildIndex = 0;
            m_count = 0;
        }

        /// <summary>
        /// Add child node at the end of children
        /// </summary>
        /// <param name="child">Node we want to push back</param>
        public void AddChild (Node child)
        {
            if (Count == m_children.Length)
            {
                Node[] children = new Node[m_children.Length + 1];
                Buffer.BlockCopy(m_children, 0, children, 0, m_children.Length);
                children[m_children.Length] = child;
                m_children = children;
            }
            else
            {
                m_children[m_count++] = child;
            }
        }

        /// <summary>
        /// Remove child with its node name
        /// </summary>
        /// <param name="name">Name of the node will be removed</param>
        public void RemoveChild (string name)
        {
            int index = Array.FindIndex<Node>(m_children, delegate (Node child)
            {
                return child.NodeName == name;
            });

            // contain
            if (index != -1)
            {
                Node[] children = new Node[m_children.Length - 1];
                Buffer.BlockCopy(m_children, 0, children, 0, index);
                Buffer.BlockCopy(m_children, index + 1, children, index, Count - index - 1);
                m_children = children;
            }
        }

        protected override void OnInitialize ( )
        {
            if (m_children.Length > Count)
            {
                Node[] children = new Node[Count];
                Buffer.BlockCopy(m_children, 0, children, 0, Count);
                m_children = children;
            }
        }

    }

    /// <summary>
    /// Go ahead until one is failed
    /// </summary>
    sealed public class SequenceNode : CompositeNode
    {
        public override Common.NodeExecuteState Execute ( )
        {
            while (true)
            {
                Common.NodeExecuteState state = m_children[m_currentChildIndex].Execute();

                // return if is running or this child has failed out
                if (state != Common.NodeExecuteState.g_kSuccess)
                {
                    return state;
                }

                // all chilren have finished
                else if (++m_currentChildIndex == m_children.Length ||
                    m_children[m_currentChildIndex] == null)
                {
                    return Common.NodeExecuteState.g_kSuccess;
                }

            }
        }
    }

    /// <summary>
    /// Go ahead until one is successful
    /// </summary>
    sealed public class SelectorNode : CompositeNode
    {
        public override Common.NodeExecuteState Execute ( )
        {
            while (true)
            {
                Common.NodeExecuteState state = m_children[m_currentChildIndex].Execute();

                // return if it is running or we have find a succees one
                if (state != Common.NodeExecuteState.g_kFailure)
                {
                    return state;
                }

                // no one was successful
                else if (++m_currentChildIndex == m_children.Length ||
                    m_children[m_currentChildIndex] == null)
                {
                    return Common.NodeExecuteState.g_kFailure;
                }
            }
        }
    }

    public class HintedSelectorNode : CompositeNode
    {
        public override Common.NodeExecuteState Execute ( )
        {
            throw new NotImplementedException();
        }

        virtual public void Hinted (Common.HintType hintType, string nodeName) { }
    }

    public class ParallelNode : CompositeNode
    {
        public override Common.NodeExecuteState Execute ( )
        {
            throw new NotImplementedException();
        }
    }

    #endregion Composite

    abstract public class BehaviourNode : Node { }



}
