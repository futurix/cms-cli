using System;

namespace Wave.Common
{
    public struct Pair<T, U>
    {
        public T First { get; private set; }
        public U Second { get; private set; }
        
        public Pair(T first, U second)
            : this()
        {
            First = first;
            Second = second;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", First.ToString(), Second.ToString());
        }
    }
}
