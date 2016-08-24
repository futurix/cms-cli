using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Wave.Common;
using Wave.Services;

namespace Wave.Platform.Messaging
{
    public class FieldList : IEnumerable<IFieldBase>, IFieldBase, ICacheable
    {
        public const int FieldNotFound = -1;
        
        public List<IFieldBase> Data
        {
            get { return fields; }
            set { fields = value; }
        }

        private List<IFieldBase> fields = new List<IFieldBase>();
        private short fieldID = -1;

        public FieldList()
        {
        }

        public FieldList(Stream str, short numberOfFields)
            : this()
        {
            ReadFromStream(str, numberOfFields);
        }

        public FieldList(List<IFieldBase> value)
            : this()
        {
            fields = value;
        }

        public IFieldBase this[int index]
        {
            get { return fields[index]; }
        }

        public IFieldBase this[short fID]
        {
            get
            {
                if (fields.Count > 0)
                {
                    foreach (IFieldBase weField in fields)
                    {
                        if (weField.FieldID == fID)
                            return weField;
                    }
                }

                return null;
            }
        }

        public IFieldBase this[Enum fID]
        {
            get { return this[Convert.ToInt16(fID)]; }
        }

        public int DataPackedLength
        {
            get
            {
                int res = 0;

                if (fields.Count > 0)
                {
                    foreach (IFieldBase weField in fields)
                        res += weField.PackedSize;
                }

                return res;
            }
        }

        public int Count
        {
            get { return fields.Count; }
        }

        public override string ToString()
        {
            return String.Format("Field-list: {0} items", Count);
        }

        private void ReadFromStream(Stream str, short numberOfFields)
        {
            if (numberOfFields <= 0)
                return;
            
            for (int i = 0; i < numberOfFields; i++)
            {
                // start with the field type
                int b1 = str.ReadByte();
                FieldType fType = (FieldType)(b1 >> 4);
                short wID = (short)((b1 & 0x000F) << 8);
                wID |= (short)str.ReadByte();

                switch (fType)
                {
                    case FieldType.Int16:
                        fields.Add(new Int16Field(wID, str));
                        break;

                    case FieldType.Int32:
                        fields.Add(new Int32Field(wID, str));
                        break;

                    case FieldType.Bool:
                        fields.Add(new BooleanField(wID, str));
                        break;

                    case FieldType.Double:
                        fields.Add(new DoubleField(wID, str));
                        break;

                    case FieldType.Byte:
                        fields.Add(new ByteField(wID, str));
                        break;

                    case FieldType.Binary:
                        fields.Add(new BinaryField(wID, str));
                        break;

                    case FieldType.DateTime:
                        fields.Add(new DateTimeField(wID, str));
                        break;

                    case FieldType.LongBinary:
                        fields.Add(new LongBinaryField(wID, str));
                        break;

                    case FieldType.String:
                        fields.Add(new StringField(wID, str));
                        break;

                    case FieldType.FieldList:
                        fields.Add(FieldList.CreateField(wID, str, str.ReadShort()));
                        break;

                    default:
                        DebugHelper.Out("Unsupported field type: {0}", (int)fType);
                        break;
                }
            }
        }

        #region Shortcuts

        public void Add(IFieldBase field)
        {
            fields.Add(field);
        }

        public void AddBinary(short fID, byte[] value)
        {
            Add(new BinaryField(fID, value));
        }

        public void AddBinary(Enum fID, byte[] value)
        {
            Add(new BinaryField(Convert.ToInt16(fID), value));
        }

        public void AddBoolean(short fID, bool value)
        {
            Add(new BooleanField(fID, value));
        }

        public void AddBoolean(Enum fID, bool value)
        {
            Add(new BooleanField(Convert.ToInt16(fID), value));
        }

        public void AddByte(short fID, byte value)
        {
            Add(new ByteField(fID, value));
        }

        public void AddByte(Enum fID, byte value)
        {
            Add(new ByteField(Convert.ToInt16(fID), value));
        }

        public void AddDateTime(short fID, DateTime value)
        {
            Add(new DateTimeField(fID, value));
        }

        public void AddDateTime(Enum fID, DateTime value)
        {
            Add(new DateTimeField(Convert.ToInt16(fID), value));
        }

        public void AddDateTime(short fID, long value)
        {
            Add(new DateTimeField(fID, value));
        }

        public void AddDateTime(Enum fID, long value)
        {
            Add(new DateTimeField(Convert.ToInt16(fID), value));
        }

        public void AddInt16(short fID, short value)
        {
            Add(new Int16Field(fID, value));
        }

        public void AddInt16(Enum fID, short value)
        {
            Add(new Int16Field(Convert.ToInt16(fID), value));
        }

        public void AddInt32(short fID, int value)
        {
            Add(new Int32Field(fID, value));
        }

        public void AddInt32(Enum fID, int value)
        {
            Add(new Int32Field(Convert.ToInt16(fID), value));
        }

        public void AddLongBinary(short fID, byte[] value)
        {
            Add(new LongBinaryField(fID, value));
        }

        public void AddLongBinary(Enum fID, byte[] value)
        {
            Add(new LongBinaryField(Convert.ToInt16(fID), value));
        }

        public void AddString(short fID, string value)
        {
            Add(new StringField(fID, value));
        }

        public void AddString(Enum fID, string value)
        {
            Add(new StringField(Convert.ToInt16(fID), value));
        }

        #endregion

        #region Field search methods

        public bool HasField(short fID)
        {
            foreach (IFieldBase field in fields)
            {
                if (field.FieldID == fID)
                    return true;
            }

            return false;
        }

        public bool HasField(Enum fID)
        {
            return HasField(Convert.ToInt16(fID));
        }

        public List<IFieldBase> GetItems(short fID)
        {
            List<IFieldBase> res = new List<IFieldBase>();

            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].FieldID == fID)
                    res.Add(fields[i]);
            }

            return res;
        }

        public List<IFieldBase> GetItems(Enum fID)
        {
            return GetItems(Convert.ToInt16(fID));
        }

        public List<T> GetItems<T>(short fID)
            where T : class, IFieldBase
        {
            List<T> res = new List<T>();

            for (int i = 0; i < fields.Count; i++)
            {
                if ((fields[i].FieldID == fID) && (fields[i] is T))
                    res.Add((T)fields[i]);
            }

            return res;
        }

        public List<T> GetItems<T>(Enum fID)
            where T : class, IFieldBase
        {
            return GetItems<T>(Convert.ToInt16(fID));
        }

        public int GetItemCount(short fID)
        {
            int res = 0;

            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].FieldID == fID)
                    res++;
            }

            return res;
        }

        public int GetItemCount(Enum fID)
        {
            return GetItemCount(Convert.ToInt16(fID));
        }

        public PairedList<T, U> GetPairedItems<T, U>(short firstID, short secondID)
            where T : class, IFieldBase
            where U : class, IFieldBase
        {
            PairedList<T, U> res = new PairedList<T, U>();

            for (int i = 0; i < (fields.Count - 1); i++)
            {
                if ((fields[i].FieldID == firstID) && (fields[i + 1].FieldID == secondID) &&
                    (fields[i] is T) && (fields[i + 1] is U))
                {
                    res.Add(new Pair<T, U>((T)fields[i], (U)fields[i + 1]));
                    i++;
                }
            }

            return res;
        }

        public PairedList<T, U> GetPairedItems<T, U>(Enum firstID, Enum secondID)
            where T : class, IFieldBase
            where U : class, IFieldBase
        {
            return GetPairedItems<T, U>(Convert.ToInt16(firstID), Convert.ToInt16(secondID));
        }

        public PairedList<T, U> GetPairedItemsOrNull<T, U>(short firstID, short secondID)
            where T : class, IFieldBase
            where U : class, IFieldBase
        {
            PairedList<T, U> res = new PairedList<T, U>();

            for (int i = 0; i < fields.Count; i++)
            {
                if ((fields[i].FieldID == firstID) && (fields[i] is T))
                {
                    if (((i + 1) < fields.Count) && ((fields[i + 1].FieldID == secondID) && (fields[i + 1] is U)))
                    {
                        res.Add(new Pair<T, U>((T)fields[i], (U)fields[i + 1]));
                        i++;
                    }
                    else
                        res.Add(new Pair<T, U>((T)fields[i], null));
                }
            }

            return res;
        }

        public PairedList<T, U> GetPairedItemsOrNull<T, U>(Enum firstID, Enum secondID)
            where T : class, IFieldBase
            where U : class, IFieldBase
        {
            return GetPairedItemsOrNull<T, U>(Convert.ToInt16(firstID), Convert.ToInt16(secondID));
        }

        public T GetNextItemAfter<T>(short fID, short followupID)
            where T : class, IFieldBase
        {
            for (int i = 0; i < (fields.Count - 1); i++)
            {
                if (fields[i].FieldID == fID)
                {
                    if ((fields[i + 1].FieldID == followupID) && (fields[i + 1] is T))
                        return (T)fields[i + 1];

                    break;
                }
            }
            
            return null;
        }

        public T GetNextItemAfter<T>(Enum fID, Enum followupID)
            where T : class, IFieldBase
        {
            return GetNextItemAfter<T>(Convert.ToInt16(fID), Convert.ToInt16(followupID));
        }

        public Pair<T, U> GetPair<T, U>(short firstID, short secondID)
            where T : class, IFieldBase
            where U : class, IFieldBase
        {
            for (int i = 0; i < (fields.Count - 1); i++)
            {
                if ((fields[i].FieldID == firstID) && (fields[i + 1].FieldID == secondID) &&
                    (fields[i] is T) && (fields[i + 1] is U))
                {
                    return new Pair<T, U>((T)fields[i], (U)fields[i + 1]);
                }
            }

            return new Pair<T, U>();
        }

        public Pair<T, U> GetPair<T, U>(Enum firstID, Enum secondID)
            where T : class, IFieldBase
            where U : class, IFieldBase
        {
            return GetPair<T, U>(Convert.ToInt16(firstID), Convert.ToInt16(secondID));
        }

        #endregion

        #region Field packing

        public void EncodeFields(Stream str)
        {
            if (fields.Count > 0)
            {
                foreach (IFieldBase weField in fields)
                    weField.Pack(str);
            }
        }

        public byte[] ToByteArray()
        {
            using (MemoryStream mem = new MemoryStream())
            {
                EncodeFields(mem);

                return mem.ToArray();
            }
        }

        #endregion

        #region IEnumerable<T> implementation

        public IEnumerator<IFieldBase> GetEnumerator()
        {
            return fields.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return fields.GetEnumerator();
        }

        #endregion

        #region ICacheable implementation

        public CacheableType StoredType
        {
            get { return CacheableType.FieldList; }
        }

        public void Persist(Stream str)
        {
            str.WriteByte(0);

            // number of fields
            str.WriteShort((short)fields.Count);

            // fields
            EncodeFields(str);
        }

        public void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
            {
                short numberOfFields = str.ReadShort();

                if (numberOfFields > 0)
                    ReadFromStream(str, numberOfFields);
            }
        }

        #endregion

        #region IFieldBase implementation and supporting methods

        short IFieldBase.FieldID
        {
            get { return fieldID; }
            set { fieldID = value; }
        }

        int IFieldBase.DataLength
        {
            get
            {
                int res = 0;

                if (fields.Count > 0)
                {
                    foreach (IFieldBase weField in fields)
                        res += weField.DataLength;
                }

                return res;
            }
        }

        int IFieldBase.PackedLength
        {
            get { return (DataPackedLength + sizeof(short)); }
        }

        int IFieldBase.PackedSize
        {
            get { return (DataPackedLength + sizeof(short)); }
        }

        void IFieldBase.Pack(Stream str)
        {
            IntegralFieldBase.WriteFieldHeader(str, FieldType.FieldList, fieldID);
            str.WriteShort((short)fields.Count);

            EncodeFields(str);
        }

        public bool IsValidField
        {
            get { return (fieldID != -1); }
        }

        #endregion

        #region Helper methods

        public static FieldList CreateField(short fID)
        {
            FieldList res = new FieldList();

            SetFieldID(res, fID);

            return res;
        }

        public static FieldList CreateField(Enum fID)
        {
            return CreateField(Convert.ToInt16(fID));
        }

        public static FieldList CreateField(short fID, Stream str, short numberOfFields)
        {
            FieldList res = new FieldList(str, numberOfFields);

            SetFieldID(res, fID);

            return res;
        }

        public static FieldList CreateField(Enum fID, Stream str, short numberOfFields)
        {
            return CreateField(Convert.ToInt16(fID), str, numberOfFields);
        }

        public static FieldList CreateField(short fID, List<IFieldBase> value)
        {
            FieldList res = new FieldList(value);

            SetFieldID(res, fID);

            return res;
        }

        public static FieldList CreateField(Enum fID, List<IFieldBase> value)
        {
            return CreateField(Convert.ToInt16(fID), value);
        }

        private static void SetFieldID(FieldList target, short fID)
        {
            if (target != null)
                ((IFieldBase)target).FieldID = fID;
        }

        #endregion
    }
}
