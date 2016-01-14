/*************************************************************

** Auth: ysd
** Date: 2016.1.8
** Desc: Behaviour node, leaf node
** Vers: v1.0

*************************************************************/

using System;

namespace HeBT
{

    /// <summary>
    /// Execute some action or condition related to game logic;
    /// Leaf node.
    /// </summary>
    abstract public class BehaviourNode : Node
    {
        public BehaviourNode (string name)
                : base(name)
        { }
    }

    /// <summary>
    /// Send a hint, always success.
    /// </summary>
    public class HinterNode : BehaviourNode
    {

        private Hint m_hint;

        public HinterNode (string name, Hint hint)
            : base(name)
        {
            m_hint = hint;
        }

        public override Common.NodeExecuteState Execute ( )
        {
            m_hint.Send();
            return Common.NodeExecuteState.g_kSuccess;    
        }

    }

    /// <summary>
    /// Concrete condition and relative paremeter will be writen in child class.
    /// </summary>
    abstract public class ConditionNode : BehaviourNode
    {

        public ConditionNode (string name)
                : base(name)
        { }

        public override Common.NodeExecuteState Execute ( )
        {
            if (Check())
                return Common.NodeExecuteState.g_kSuccess;
            else
                return Common.NodeExecuteState.g_kFailure;
        }

        abstract public bool Check ( );
    }

    /// <summary>
    /// Example float-comparer condition node;
    /// Return true if input is bigger/smaller/equal then the given value.
    /// </summary>
    public class FloatConditionNode : ConditionNode
    {
        public delegate bool DelFloatCmper (float input, float param);

        private DelFloatCmper m_cmper;

        public DelFloatCmper Cmper
        {
            set
            {
                m_cmper = value;
            }
        }

        public delegate float DelGetInput ( );

        /// <summary>
        /// Get input from external game logic, such as blackboard 
        /// </summary>
        private DelGetInput m_getInputMethod;

        public DelGetInput GetInputMethod
        {
            set
            {
                m_getInputMethod = value;
            }
        }

        /// <summary>
        /// Fixed parameter
        /// </summary>
        public float param;

        public FloatConditionNode (string name, float p, DelFloatCmper cmp)
            : base(name)
        {
            param = p;
            m_cmper = cmp;
        }

        public override bool Check ( )
        {
            return m_cmper(m_getInputMethod(), param);
        }
    }
}
