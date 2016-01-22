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

        /// <summary>
        /// The Blackboard at the parent node of this Blackboard's node.
        /// </summary>
        private Blackboard m_parent;

        public Blackboard Parent
        {
            set
            {
                m_parent = value;
            }
        }

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

        public Blackboard (Dictionary<string, object> initDatas)
        {
            m_blackboardData = new Dictionary<string, object>(initDatas);
            List<string> names = new List<string>(initDatas.Keys);
            for (int i = 0; i < names.Count; i++)
            {
                m_onDataChangeHandler.Add(names[i], null);
            }
        }

        /// <summary>
        /// Get data of a value.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="name">Key name.</param>
        /// <returns>False if there is no desired value.</returns>
        public bool Get<T> (string name, out T value)
        {
            // It have the key.
            if (m_blackboardData.ContainsKey(name))
            {
                try
                {
                    // the value with given name will have a unexcepted type
                    value = (T)m_blackboardData[name];
                }
                catch (System.InvalidCastException)
                {
                    if (m_parent == null)
                    {
                        value = default(T);
                        return false;
                    }
                    else
                        return m_parent.Get(name, out value);
                }
                return true;
            }

            // It do not have the key, but its parent may have
            if (m_parent == null)
            {
                value = default(T);
                return false;
            }
            else
                return m_parent.Get(name, out value);
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

            if (!m_onDataChangeHandler.ContainsKey(name) && m_blackboardData.ContainsKey(name))
            {
                m_onDataChangeHandler[name] = onDataChangeHandler;
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
