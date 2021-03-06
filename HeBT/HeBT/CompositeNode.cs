﻿/*************************************************************

** Auth: ysd
** Date: 2016.1.8
** Desc: Composite node
** Vers: v1.0

*************************************************************/

using System;
using System.Collections.Generic;

namespace HeBT
{

    /// <summary>
    /// A composite node controll the process of logic flow in BT
    /// </summary>
    abstract public class CompositeNode : NonLeafNode
    {
        protected byte m_currentChildIndex;

        public byte CurrentChildIndex
        {
            set
            {
                m_currentChildIndex = value;
            }
        }

        protected bool m_inited = false;

        protected CompositeNode (string name, byte capacity)
            : base(name)
        {
            m_childList = new List<Node>(capacity);
            m_currentChildIndex = 0;
        }

        private List<Node> m_childList;

        /// <summary>
        /// Add child node at the end of children
        /// </summary>
        public void AddChild (Node child)
        {
            child.Parent = this;
            m_childList.Add(child);
        }

        /// <summary>
        /// Add child nodes at the end of children
        /// </summary>
        public void AddChildren (params Node[] children)
        {
            for (int i = 0; i < children.Length; i++)
            {
                AddChild(children[i]);
            }
        }

        /// <summary>
        /// Remove child with its node name
        /// </summary>
        /// <param name="name">Name of the node will be removed</param>
        public void RemoveChild (string name)
        {
            int index = m_childList.FindIndex(delegate (Node child)
            {
                return child.NodeName == name;
            });

            m_childList.RemoveAt(index);
        }

        /// <summary>
        /// Array is faster than List<T>
        /// </summary>
        internal virtual void Complete ( )
        {
            m_children = m_childList.ToArray();
            m_childList.Clear();
            m_childList.TrimExcess();
            m_childList = null;
        }

    }

    /// <summary>
    /// Go ahead until one is failed.
    /// </summary>
    public sealed class SequenceNode : CompositeNode
    {

        internal SequenceNode (string name, byte length)
        : base(name, length)
        { }

        public override Common.NodeExecuteState Execute (Blackboard blackboard)
        {

            if (!m_inited)
            {
                OnInitialize();
                m_inited = true;
            }

            while (true)
            {
                Common.NodeExecuteState state;
                if (privateBlackboard == null)
                    // pass the blackboard from parent
                    state = m_children[m_currentChildIndex].Execute(blackboard);
                else
                    state = m_children[m_currentChildIndex].Execute(privateBlackboard);

                // return if is running or this child has failed out
                if (state != Common.NodeExecuteState.kSuccess)
                {
                    return state;
                }

                // all chilren have finished
                else if (++m_currentChildIndex == m_children.Length ||
                    m_children[m_currentChildIndex] == null)
                {
                    m_currentChildIndex = 0;
                    return Common.NodeExecuteState.kSuccess;
                }

            }
        }
    }

    /// <summary>
    /// Go ahead until one is successful, then execute the next child.
    /// </summary>
    public sealed class SelectorNode : CompositeNode
    {

        internal SelectorNode (string name, byte length)
            : base(name, length)
        { }

        public override Common.NodeExecuteState Execute (Blackboard blackboard)
        {

            if (!m_inited)
            {
                OnInitialize();
                m_inited = true;
            }

            while (true)
            {
                Common.NodeExecuteState state;
                if (privateBlackboard == null)
                    state = m_children[m_currentChildIndex].Execute(blackboard);
                else
                    state = m_children[m_currentChildIndex].Execute(privateBlackboard);

                // return if it is running or we have find a succees one
                if (state != Common.NodeExecuteState.kFailure)
                {
                    return state;
                }

                // no one was successful
                else if (++m_currentChildIndex == m_children.Length ||
                    m_children[m_currentChildIndex] == null)
                {
                    m_currentChildIndex = 0;
                    return Common.NodeExecuteState.kFailure;
                }
            }
        }
    }

    /// <summary>
    /// Go ahead until one is successful, then execute the first child.
    /// </summary>
    public sealed class ReSelectorNode : CompositeNode
    {

        internal ReSelectorNode (string name, byte length)
            : base(name, length)
        { }

        public override Common.NodeExecuteState Execute (Blackboard blackboard)
        {
            if (!m_inited)
            {
                OnInitialize();
                m_inited = true;
            }

            while (true)
            {
                Common.NodeExecuteState state;
                if (privateBlackboard == null)
                    state = m_children[m_currentChildIndex].Execute(blackboard);
                else
                    state = m_children[m_currentChildIndex].Execute(privateBlackboard);

                // return if it is running
                if (state == Common.NodeExecuteState.kRunning)
                {
                    return state;
                }

                // return and re-execute the first child if current child is successful
                else if (state == Common.NodeExecuteState.kSuccess)
                {
                    m_currentChildIndex = 0;
                    return state;
                }

                // no one was successful
                else if (++m_currentChildIndex == m_children.Length ||
                    m_children[m_currentChildIndex] == null)
                {
                    m_currentChildIndex = 0;
                    return Common.NodeExecuteState.kFailure;
                }
            }
        }

    }

    /// <summary>
    /// Handle the hint.
    /// </summary>
    internal class HintReceiver
    {

        /// <summary>
        /// Reorder the children of a hinted selector
        /// </summary>
        /// <param name="childIndex">The origin child index</param>
        /// <param name="hint">Hint type</param>
        /// <param name="executeOrder">execution order of a hinted selector</param>
        static public void Hinted (byte childIndex, Common.HintType hint, ref byte[] executeOrder)
        {
            switch (hint)
            {
                case Common.HintType.kPositive:

                    // reorder
                    for (byte i = childIndex; i > 0; i--)
                    {
                        executeOrder[i] = executeOrder[i - 1];
                    }
                    executeOrder[0] = childIndex;
                    break;
                case Common.HintType.kNeutral:
                    byte currentIndex = (byte)Array.IndexOf<byte>(executeOrder, childIndex);
                    // if hinted child is not at its origin order, reorder it back
                    if (currentIndex < childIndex)
                    {
                        for (byte i = currentIndex; i < childIndex; i++)
                        {
                            executeOrder[i] = executeOrder[i + 1];
                        }
                        executeOrder[childIndex] = childIndex;
                    }
                    else if (currentIndex > childIndex)
                    {
                        for (byte i = currentIndex; i > childIndex; i--)
                        {
                            executeOrder[i] = executeOrder[i - 1];
                        }
                        executeOrder[childIndex] = childIndex;
                    }
                    break;
                case Common.HintType.kNegative:
                    // reorder
                    for (byte i = childIndex; i < executeOrder.Length - 1; i++)
                    {
                        executeOrder[i] = executeOrder[i + 1];
                    }
                    executeOrder[executeOrder.Length - 1] = childIndex;
                    break;
            }
        }
    }

    /// <summary>
    /// Go ahead until one is successful, then execute the next child; 
    /// Hinted selector node can reorder its children when receive hints from high-layer logic.
    /// </summary>
    public sealed class HintedSelectorNode : CompositeNode
    {

        private byte[] m_executeOrder;

        internal HintedSelectorNode (string name, byte length)
            : base(name, length)
        { }

        internal override void Complete ( )
        {
            base.Complete();
            m_executeOrder = new byte[m_children.Length];
            for (byte i = 0; i < m_children.Length; i++)
            {
                m_executeOrder[i] = i;
            }
        }

        public override Common.NodeExecuteState Execute (Blackboard blackboard)
        {
            if (!m_inited)
            {
                OnInitialize();
                m_inited = true;
            }

            while (true)
            {
                byte executeIndex = m_executeOrder[m_currentChildIndex];
                Common.NodeExecuteState state;
                if (privateBlackboard == null)
                    state = m_children[executeIndex].Execute(blackboard);
                else
                    state = m_children[executeIndex].Execute(privateBlackboard);

                // return if it is running or we have find a succees one
                if (state != Common.NodeExecuteState.kFailure)
                {
                    return state;
                }

                // no one was successful
                else if (++m_currentChildIndex == m_children.Length ||
                    m_children[m_currentChildIndex] == null)
                {
                    m_currentChildIndex = 0;
                    return Common.NodeExecuteState.kFailure;
                }
            }
        }

        /// <summary>
        /// Reorder the execution-order of hinted selector, do not change the origin order of BT
        /// </summary>
        /// <param name="childIndex">The origin child index in the BT</param>
        /// <param name="hint">Hint type</param>
        public void Hinted (byte childIndex, Common.HintType hint)
        {
            HintReceiver.Hinted(childIndex, hint, ref m_executeOrder);

            if (hint == Common.HintType.kPositive)
            {
                // the hinted child will execute at next tick 
                m_currentChildIndex = 0;
            }
        }
    }

    /// <summary>
    /// Go ahead until one is successful, then re-execute the first child; 
    /// Hinted selector node can reorder its children when receive hints from high-layer logic.
    /// </summary>
    public sealed class ReHintedSelectorNode : CompositeNode
    {
        private byte[] m_executeOrder;

        internal ReHintedSelectorNode (string name, byte length)
            : base(name, length)
        { }

        protected override void OnInitialize ( )
        {
            base.OnInitialize();
            m_executeOrder = new byte[m_children.Length];
            for (byte i = 0; i < m_children.Length; i++)
            {
                m_executeOrder[i] = i;
            }
        }

        public override Common.NodeExecuteState Execute (Blackboard blackboard)
        {
            if (!m_inited)
            {
                OnInitialize();
                m_inited = true;
            }

            while (true)
            {
                byte executeIndex = m_executeOrder[m_currentChildIndex];
                Common.NodeExecuteState state;
                if (privateBlackboard == null)
                    state = m_children[executeIndex].Execute(blackboard);
                else
                    state = m_children[executeIndex].Execute(privateBlackboard);

                // return if it is running
                if (state == Common.NodeExecuteState.kRunning)
                {
                    return state;
                }

                // return and re-execute the first child if current child is successful
                else if (state == Common.NodeExecuteState.kSuccess)
                {
                    // the hinted child is at m_executeOrder[0]
                    m_currentChildIndex = 0;
                    return state;
                }

                // no one was successful
                else if (++m_currentChildIndex == m_children.Length ||
                    m_children[m_currentChildIndex] == null)
                {
                    m_currentChildIndex = 0;
                    return Common.NodeExecuteState.kFailure;
                }
            }
        }

        /// <summary>
        /// Reorder the execution-order of hinted selector, do not change the origin order of BT
        /// </summary>
        /// <param name="childIndex">The origin child index in the BT</param>
        /// <param name="hint">Hint type</param>
        public void Hinted (byte childIndex, Common.HintType hint)
        {
            HintReceiver.Hinted(childIndex, hint, ref m_executeOrder);

            if (hint == Common.HintType.kPositive)
            {
                // the hinted child will execute at next tick 
                m_currentChildIndex = 0;
            }
        }
    }

    /// <summary>
    /// Parallelly running;
    /// Success when all children success, running when some children is running, failed when others;
    /// Every child only runs one time.
    /// </summary>
    public sealed class ParallelNodeOnceAll : CompositeNode
    {

        /// <summary>
        /// Running children after a tick
        /// </summary>
        private List<byte> m_runningChildrenIndex;

        private byte m_successNumber;
        private byte m_failureNumber;

        internal ParallelNodeOnceAll (string name, byte length)
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

        public override Common.NodeExecuteState Execute (Blackboard blackboard)
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
                Common.NodeExecuteState state;
                if (privateBlackboard == null)
                    state = m_children[m_currentChildIndex].Execute(blackboard);
                else
                    state = m_children[m_currentChildIndex].Execute(privateBlackboard);

                if (state == Common.NodeExecuteState.kSuccess)
                {
                    if (++m_successNumber == m_children.Length)
                    {
                        ResetRunningChildren();
                        return Common.NodeExecuteState.kSuccess;
                    }

                    // remove from running children list
                    m_runningChildrenIndex.RemoveAt(i);
                }
                else if (state == Common.NodeExecuteState.kFailure)
                {
                    if (++m_failureNumber == m_children.Length)
                    {
                        ResetRunningChildren();
                        return Common.NodeExecuteState.kFailure;
                    }

                    // remove from running children list
                    m_runningChildrenIndex.RemoveAt(i);
                }
            }

            // still execute the running children
            return Common.NodeExecuteState.kRunning;

        }
    }

    /// <summary>
    /// Success when some children success, failed once one child is failed
    /// </summary>
    public sealed class ParallelSeqNode : CompositeNode
    {

        private byte m_maxSuccessNumber;

        internal ParallelSeqNode (string name, byte capacity, byte maxSuccessNumber = 255)
            : base(name, capacity)
        {
            m_maxSuccessNumber = maxSuccessNumber;
        }

        public override Common.NodeExecuteState Execute (Blackboard blackboard)
        {
            if (!m_inited)
            {
                OnInitialize();
                if (m_maxSuccessNumber == 255)
                    m_maxSuccessNumber = (byte)m_children.Length;
                m_inited = false;
            }

            int successNumber = 0;
            int l = m_children.Length;
            for (int i = 0; i < l; i++)
            {
                Common.NodeExecuteState state;
                if (privateBlackboard == null)
                    state = m_children[m_currentChildIndex].Execute(blackboard);
                else
                    state = m_children[m_currentChildIndex].Execute(privateBlackboard);

                if (state == Common.NodeExecuteState.kFailure)
                {
                    // Failed once any child is failed
                    return state;
                }

                if (state == Common.NodeExecuteState.kSuccess)
                {
                    successNumber++;
                }
            }

            // Success when some children success
            if (successNumber == m_maxSuccessNumber)
            {
                return Common.NodeExecuteState.kSuccess;
            }

            return Common.NodeExecuteState.kRunning;
        }
    }

    /// <summary>
    /// Success once one child is successful, failed when some children is failed
    /// </summary>
    public sealed class ParallelSelNode : CompositeNode
    {

        private byte m_maxFailedNumber;

        internal ParallelSelNode (string name, byte length, byte maxFailedNumber)
             : base(name, length)
        {
            m_maxFailedNumber = maxFailedNumber;
        }

        public override Common.NodeExecuteState Execute (Blackboard blackboard)
        {

            if (!m_inited)
            {
                OnInitialize();
                if (m_maxFailedNumber == 255)
                    m_maxFailedNumber = (byte)m_children.Length;
                m_inited = false;
            }

            int failedNumber = 0;
            int l = Children.Length;
            for (int i = 0; i < l; i++)
            {
                Common.NodeExecuteState state;
                if (privateBlackboard == null)
                    state = m_children[m_currentChildIndex].Execute(blackboard);
                else
                    state = m_children[m_currentChildIndex].Execute(privateBlackboard);

                // success once one child is successful
                if (state == Common.NodeExecuteState.kSuccess)
                {
                    return Common.NodeExecuteState.kSuccess;
                }

                if (state == Common.NodeExecuteState.kFailure)
                {
                    failedNumber++;
                }
            }

            // Success when some children is failed
            if (failedNumber == m_maxFailedNumber)
            {
                return Common.NodeExecuteState.kFailure;
            }

            return Common.NodeExecuteState.kRunning;

        }

    }

}
