/*************************************************************

** Auth: ysd
** Date: 2015.12.23
** Desc: Hint send from high-layer logic to low-layer logic;
         Like command-patterns.
** Vers: v1.0

*************************************************************/

namespace HeBT
{

    public class Hint
    {
        private string m_nodeName;
        private Common.HintType m_hintType;

        public string NodeName
        {
            get
            {
                return m_nodeName;
            }
        }

        public Common.HintType HintType
        {
            get
            {
                return m_hintType;
            }
        }

        private BehaviourTree m_receiver;

        /// <summary>
        /// Invoker will assign a receiver low behaviour tree when create a hint
        /// </summary>
        public Hint(BehaviourTree receiver, string name, Common.HintType hint)
        {
            m_nodeName = name;
            m_hintType = hint;
            m_receiver = receiver;
        }

        public void Send()
        {
            m_receiver.ReceiveHint(m_nodeName, m_hintType);
        }
    }
}
