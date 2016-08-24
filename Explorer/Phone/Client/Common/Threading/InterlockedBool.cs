using System.Threading;

namespace Wave.Common
{
    public struct InterlockedBool
    {
        public bool Value
        {
            get { return (internalValue == 1); }
            set { Interlocked.Exchange(ref internalValue, value ? 1 : 0); }
        }

        #region Private fields

        private int internalValue;

        #endregion

        public InterlockedBool(bool input)
            : this()
        {
            Value = input;
        }

        public bool Exchange(bool newValue)
        {
            return (Interlocked.Exchange(ref internalValue, newValue ? 1 : 0) == 1);
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
        
        public static implicit operator InterlockedBool(bool arg)
        {
            return new InterlockedBool(arg);
        }

        public static implicit operator bool(InterlockedBool arg)
        {
            return arg.Value;
        }
    }
}
