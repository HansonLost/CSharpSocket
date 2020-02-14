using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig
{
    public class Listener<T0>
    {
        public delegate void Callback(T0 t0);
        private Callback m_Callback;
        public void AddListener(Callback func)
        {
            m_Callback += func;
        }
        public void RemoveListener(Callback func)
        {
            m_Callback -= func;
        }
        public void Invoke(T0 t0)
        {
            m_Callback.Invoke(t0);
        }
    }

    public class Listener<T0, T1>
    {
        public delegate void Callback(T0 t0, T1 t1);
        private Callback m_Callback;
        public void AddListener(Callback func)
        {
            m_Callback += func;
        }
        public void RemoveListener(Callback func)
        {
            m_Callback -= func;
        }
        public void Invoke(T0 t0, T1 t1)
        {
            m_Callback.Invoke(t0, t1);
        }
    }
}
