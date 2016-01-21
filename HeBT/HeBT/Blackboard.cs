/*************************************************************

** Auth: ysd
** Date: 2016.1.21
** Desc: Blackboard structrue to share data among tree node/BT.
** Vers: v1.0

*************************************************************/

using System.Collections.Generic;

namespace HeBT
{
    public class Blackboard
    {

        private Dictionary<string, object> m_blackboardData;

        public delegate void DelOnDataChange ( );
        private Dictionary<string, DelOnDataChange> m_onDataChangeHandler;

        /// <summary>
        /// A BT only get data from a Blackboard.
        /// </summary>
        public Blackboard ( )
        {
            m_blackboardData = new Dictionary<string, object>();
            m_onDataChangeHandler = new Dictionary<string, DelOnDataChange>();
        }

        /// <summary>
        /// Get data of a value.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="name"></param>
        /// <returns>False if there is no desired value.</returns>
        public bool Get<T> (string name, out T value)
        {
            if (m_blackboardData.ContainsKey(name))
            {
                try
                {
                    // the value with given name will have a unexcepted type
                    value = (T)m_blackboardData[name];
                }
                catch (System.InvalidCastException)
                {
                    throw new System.InvalidCastException("Wrong type");
                }
                return true;
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Add new value.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="name"></param>
        /// <returns>False if there has the name.</returns>
        public bool Add<T> (string name, T value)
        {
            if (m_blackboardData.ContainsKey(name))
            {
                return false;
            }

            // add value and value change event handler
            m_onDataChangeHandler.Add(name, null);
            m_blackboardData.Add(name, value);
            return true;
        }

        /// <summary>
        /// Set data for a value and trigger its handlers.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <returns>False if there is no desired value.</returns>
        public bool Set<T> (string name, T value)
        {
            if (!m_blackboardData.ContainsKey(name))
            {
                Add(name, value);
                return true;
            }

            m_onDataChangeHandler[name]();
            m_blackboardData[name] = value;
            return true;
        }

        /// <summary>
        /// Register a handler for a value.
        /// </summary>
        public bool AddListener (string name, DelOnDataChange onDataChangeHandler)
        {
            if (m_onDataChangeHandler.ContainsKey(name) && m_blackboardData.ContainsKey(name))
            {
                m_onDataChangeHandler[name] += onDataChangeHandler;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unregister a handler for a value.
        /// </summary>
        public bool RemoveListener (string name, DelOnDataChange onDataChangeHandler)
        {
            if (m_onDataChangeHandler.ContainsKey(name) && m_blackboardData.ContainsKey(name))
            {
                m_onDataChangeHandler[name] -= onDataChangeHandler;
                return true;
            }

            return false;
        }

    }
}
