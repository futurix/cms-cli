using System.Threading;

namespace Wave.Common
{
    public struct InterlockedInt
    {
        public int Value
        {
            get { return internalValue; }
            set { Interlocked.Exchange(ref internalValue, value); }
        }

        #region Private fields

        private int internalValue;

        #endregion

        public InterlockedInt(int input)
            : this()
        {
            Value = input;
        }

        public int Exchange(int newValue)
        {
            return Interlocked.Exchange(ref internalValue, newValue);
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator InterlockedInt(int arg)
        {
            return new InterlockedInt(arg);
        }

        public static implicit operator int(InterlockedInt arg)
        {
            return arg.Value;
        }
    }
}
