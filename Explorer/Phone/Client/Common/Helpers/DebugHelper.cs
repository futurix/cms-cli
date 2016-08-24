using System;
using System.Diagnostics;
using Wave.Platform.Messaging;

namespace Wave.Common
{
    public static class DebugHelper
    {
        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
        
        [Conditional("DEBUG")]
        public static void Assert(bool condition, string errorMessage)
        {
            if (!condition)
                Debug.WriteLine(errorMessage);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string errorMessage, params object[] args)
        {
            if (!condition)
                Debug.WriteLine(errorMessage, args);
        }
        
        [Conditional("DEBUG")]
        public static void Out(string message)
        {
            Debug.WriteLine(message);
        }

        [Conditional("DEBUG")]
        public static void Out(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        [Conditional("WAVETRACE")]
        public static void Trace(string message)
        {
            Debug.WriteLine(message);
        }

        [Conditional("WAVETRACE")]
        public static void Trace(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        [Conditional("DEBUG")]
        public static void CheckFieldType(IFieldBase field, Type expectedType)
        {
            if ((field != null) && (expectedType != null))
            {
                if (!expectedType.IsInstanceOfType(field))
                    DebugHelper.Out("Unexpected field: expected {0}, got {1}", expectedType.Name, field.GetType().Name);
            }
        }
    }
}
