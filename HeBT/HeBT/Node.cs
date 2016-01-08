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


}
