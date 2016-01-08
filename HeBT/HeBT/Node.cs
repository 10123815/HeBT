/*************************************************************

** Auth: ysd
** Date: 2015.12.23
** Desc: Behaviour Tree node
** Vers: v1.0

*************************************************************/

using System;
using System.Collections.Generic;

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

        public Node (string name)
        {
            m_nodeName = name;
        }

        virtual protected void OnInitialize ( ) { }

        abstract public Common.NodeExecuteState Execute ( );

    }

    abstract public class NonLeafNode : Node
    {
        protected Node[] m_children;

        public Node[] Children
        {
            get
            {
                return m_children;
            }
        }

        public NonLeafNode (string nodeName)
            : base(nodeName)
        { }
    }

    #region Decorator

    /// <summary>
    /// Wrap a behaviour node with other behaviour
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
    /// Some External condition
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

    #endregion Decorator

    #region Composite

    abstract public class CompositeNode : NonLeafNode
    {
        protected byte m_currentChildIndex;

        private byte m_count;

        public byte Count
        {
            get
            {
                return m_count;
            }
        }

        protected bool m_inited = false;

        public CompositeNode (string name, byte capacity)
            : base(name)
        {
            m_children = new Node[capacity];
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

        public SequenceNode (string name, byte length)
        : base(name, length)
        { }

        public override Common.NodeExecuteState Execute ( )
        {

            if (!m_inited)
            {
                OnInitialize();
                m_inited = true;
            }

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

        public SelectorNode (string name, byte length)
            : base(name, length)
        { }

        public override Common.NodeExecuteState Execute ( )
        {

            if (!m_inited)
            {
                OnInitialize();
                m_inited = true;
            }

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

    sealed public class HintedSelectorNode : CompositeNode
    {
        public HintedSelectorNode (string name, byte length)
            : base(name, length)
        { }

        private byte[] m_executeOrder;

        protected override void OnInitialize ( )
        {
            base.OnInitialize();
            m_executeOrder = new byte[m_children.Length];
            for (byte i = 0; i < m_children.Length; i++)
            {
                m_executeOrder[i] = i;
            }
        }

        public override Common.NodeExecuteState Execute ( )
        {
            if (!m_inited)
            {
                OnInitialize();
                m_inited = true;
            }

            while (true)
            {
                byte executeIndex = m_executeOrder[m_currentChildIndex];
                Common.NodeExecuteState state = m_children[executeIndex].Execute();

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

        /// <summary>
        /// Reorder the children of hinted selector
        /// </summary>
        /// <param name="childIndex">The origin child index</param>
        /// <param name="hint">Hint type</param>
        public void Hinted (byte childIndex, Common.HintType hint)
        {
            switch (hint)
            {
                case Common.HintType.g_kPositive:

                    // reorder
                    for (byte i = childIndex; i > 0; i--)
                    {
                        m_executeOrder[i] = m_executeOrder[i - 1];
                    }
                    m_executeOrder[0] = childIndex;

                    // execute at next tick 
                    m_currentChildIndex = 0;
                    break;
                case Common.HintType.g_kNeutral:
                    byte currentIndex = (byte)Array.IndexOf<byte>(m_executeOrder, childIndex);
                    // if hinted child is not at its origin order, reorder it back
                    if (currentIndex < childIndex)
                    {
                        for (byte i = currentIndex; i < childIndex; i++)
                        {
                            m_executeOrder[i] = m_executeOrder[i + 1];
                        }
                        m_executeOrder[childIndex] = childIndex;
                    }
                    else if (currentIndex > childIndex)
                    {
                        for (byte i = currentIndex; i > childIndex; i--)
                        {
                            m_executeOrder[i] = m_executeOrder[i - 1];
                        }
                        m_executeOrder[childIndex] = childIndex;
                    }
                    break;
                case Common.HintType.g_kNegative:
                    // reorder
                    for (byte i = childIndex; i < m_executeOrder.Length - 1; i++)
                    {
                        m_executeOrder[i] = m_executeOrder[i + 1];
                    }
                    m_executeOrder[m_executeOrder.Length - 1] = childIndex;
                    break;
            }
        }
    }

    /// <summary>
    /// Success when all children success, failed when all children is failed
    /// </summary>
    public class ParallelNode : CompositeNode
    {

        /// <summary>
        /// Running children after a tick
        /// </summary>
        private List<byte> m_runningChildrenIndex;

        private byte m_successNumber;
        private byte m_failureNumber;

        public ParallelNode (string name, byte length)
            : base(name, length)
        {
            m_successNumber = 0;
            m_failureNumber = 0;
            m_runningChildrenIndex = new List<byte>(m_children.Length);
            ResetRunningChildren();
        }

        private void ResetRunningChildren ( )
        {
            m_successNumber = 0;
            m_failureNumber = 0;
            for (byte i = 0; i < m_runningChildrenIndex.Count; i++)
                m_runningChildrenIndex.Add(i);
        }

        public override Common.NodeExecuteState Execute ( )
        {

            if (!m_inited)
            {
                OnInitialize();
                m_inited = true;
            }

            // We only execute the running children in next tick
            byte runningCount = (byte)m_runningChildrenIndex.Count;
            for (byte i = 0; i < runningCount; i++)
            {
                byte currentChildIndex = m_runningChildrenIndex[i];
                Common.NodeExecuteState state = m_children[currentChildIndex].Execute();

                if (state == Common.NodeExecuteState.g_kSuccess)
                {
                    if (++m_successNumber == m_children.Length)
                    {
                        ResetRunningChildren();
                        return Common.NodeExecuteState.g_kSuccess;
                    }

                    // remove from running children list
                    m_runningChildrenIndex.RemoveAt(i);
                }
                else if (state == Common.NodeExecuteState.g_kFailure)
                {
                    if (++m_failureNumber == m_children.Length)
                    {
                        ResetRunningChildren();
                        return Common.NodeExecuteState.g_kFailure;
                    }

                    // remove from running children list
                    m_runningChildrenIndex.RemoveAt(i);
                }
            }

            // still execute the running children
            return Common.NodeExecuteState.g_kRunning;

        }
    }

    #endregion Composite

    #region Behaviour

    /// <summary>
    /// Execute some action
    /// </summary>
    abstract public class BehaviourNode : Node
    {
        public BehaviourNode (string name)
                : base(name)
        { }
    }

    public class ConditionNode : Node
    {

        public delegate bool DelCheck ( );

        protected DelCheck m_checkMehotd;

        public ConditionNode (string name, DelCheck checkMethod)
                : base(name)
        {
            m_checkMehotd = checkMethod;
        }

        public override Common.NodeExecuteState Execute ( )
        {
            if (m_checkMehotd())
                return Common.NodeExecuteState.g_kSuccess;
            else
                return Common.NodeExecuteState.g_kFailure;
        }
    }

    #endregion Behaviour

}
