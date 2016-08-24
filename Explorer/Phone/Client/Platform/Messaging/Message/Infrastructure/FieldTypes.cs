namespace Wave.Platform.Messaging
{
    public enum FieldType
    {
        /// <summary>
        /// unknown or undefined field type
        /// </summary>
        Undefined = 0,
        
        /// <summary>
        /// string (UTF-8 encoded)
        /// </summary>
        String = 1,

        /// <summary>
        /// byte[] (contains legacy C-style string / no longer supported)
        /// </summary>
        NarrowString = 2,

        /// <summary>
        /// short
        /// </summary>
        Int16 = 3,

        /// <summary>
        /// int
        /// </summary>
        Int32 = 4,

        /// <summary>
        /// double
        /// </summary>
        Double = 5,

        /// <summary>
        /// byte
        /// </summary>
        Byte = 6,

        /// <summary>
        /// byte[] (with 16-bit size)
        /// </summary>
        Binary = 7,

        /// <summary>
        /// long (contains FILETIME value)
        /// </summary>
        DateTime = 8,

        /// <summary>
        /// uint (1 is true, 0 is false)
        /// </summary>
        Bool = 9,

        /// <summary>
        /// list of fields
        /// </summary>
        FieldList = 10,

        /// <summary>
        /// byte[] (with 24-bit size)
        /// </summary>
        LongBinary = 11
    }
}