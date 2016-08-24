using System;

namespace Wave.Common
{
    public class DataEventArgs<T> : EventArgs
    {
        public T Data { get; private set; }
        
        public DataEventArgs(T data)
        {
            Data = data;
        }
    }

    public delegate void DataEventHandler<T>(object sender, DataEventArgs<T> e);
}
