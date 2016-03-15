/*************************************************************

** Auth: ysd
** Date: 2015.12.23
** Desc: A Hinter send a hint from high-layer logic to low-layer logic.
         Exp. An admiral's BT send a command to a solider's.
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

        private HintedBehaviourTree m_receiver;

        /// <summary>
        /// Invoker will assign a receiver low behaviour tree when create a hint
        /// </summary>
        public Hint (HintedBehaviourTree receiver, string name, Common.HintType hint)
        {
            m_nodeName = name;
            m_hintType = hint;
            m_receiver = receiver;
        }

        /// <summary>
        /// Hint the receiver
        /// </summary>
        public void Send ( )
        {
            m_receiver.ReceiveHint(m_nodeName, m_hintType);
        }
    }
}
