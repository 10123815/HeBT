/*************************************************************

** Auth: ysd
** Date: 2016.1.8
** Desc: Behaviour node, leaf node, do actions or conditions
** Vers: v1.0

*************************************************************/

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

        public override Common.NodeExecuteState Execute (Blackboard blackboard)
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

        public override Common.NodeExecuteState Execute (Blackboard blackboard)
        {
            if (Check(blackboard))
                return Common.NodeExecuteState.g_kSuccess;
            else
                return Common.NodeExecuteState.g_kFailure;
        }

        abstract public bool Check (Blackboard blackboard);
    }
    
}
