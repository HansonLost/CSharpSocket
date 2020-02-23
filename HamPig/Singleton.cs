using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig
{
    public class Singleton<T> where T : class, new()
    {
        private static T m_Singleton;
        public static T instance
        {
            get
            {
                if (m_Singleton == null)
                    m_Singleton = new T();
                return m_Singleton;
            }
        }
    }
}
