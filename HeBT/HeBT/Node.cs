/*************************************************************

** Auth: ysd
** Date: 2015.12.23
** Desc: Behaviour Tree base node
** Vers: v1.0

*************************************************************/

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

        public byte ID
        {
            get
            {
                return m_id;
            }

            set
            {
                m_id = value;
            }
        }

        public NonLeafNode Parent
        {
            get
            {
                return m_parent;
            }

            set
            {
                m_parent = value;
            }
        }

        private byte m_id;

        protected NonLeafNode m_parent;

        protected Node (string name)
        {
            m_nodeName = name;
        }

        virtual protected void OnInitialize ( ) { }

        abstract public Common.NodeExecuteState Execute (Blackboard blackboard);

    }

    abstract public class NonLeafNode : Node
    {

        /// <summary>
        /// Datas shared among this node and its chidldren. Its father's other children cannot access this bb.
        /// </summary>
        public Blackboard privateBlackboard;

        protected Node[] m_children;

        public Node[] Children
        {
            get
            {
                return m_children;
            }
        }

        public NonLeafNode (string name)
            : base(name)
        { }

    }


}
