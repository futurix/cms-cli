using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class DisplayData : ICacheable
    {
        public DisplayType DisplayType { get; private set; }
        public short? Signpost { get; private set; }

        public object Data
        {
            get { return data; }
            set
            {
                if (ValidateData(value))
                    UpdateData(value);
            }
        }

        public object DownloadedData { get; private set; }

        public event EventHandler Updated;

        private object data = null;

        public DisplayData()
        {
        }

        public DisplayData(DisplayType dt, object dispData, short? sp = null)
        {
            DisplayType = dt;
            data = dispData;
            Signpost = sp;
            DownloadedData = null;
        }

        private void UpdateData(object newData)
        {
            data = newData;

            SignalUpdate();
        }

        // yes, I am THAT paranoid
        private bool ValidateData(object newData)
        {
            switch (DisplayType)
            {
                case DisplayType.String:
                    return (newData is string);

                case DisplayType.Integer:
                    return (newData is int);

                case DisplayType.ContentReference:
                    return (newData is ContentReference);

                case DisplayType.MediaMetaData:
                    return (newData is MediaMetaData);

                case DisplayType.Null:
                    return (newData == null);
            }

            return false;
        }

        public void Update(DisplayData source)
        {
            if ((source != null) && (DisplayType == source.DisplayType))
            {
                data = source.Data;

                SignalUpdate();
            }
        }

        public object OnDownloadReady(byte[] contentID, byte[] buffer)
        {
            if (IsRelevantContentReference(contentID))
            {
                DownloadedData = buffer;

                SignalUpdate();
            }

            return null;
        }

        public override string ToString()
        {
            return 
                String.Format(
                "Display data: {0}, {1}", 
                DisplayType,
                (Signpost.HasValue && (Signpost.Value >= 0)) ? "has signpost" : "no signpost");
        }

        private void SignalUpdate()
        {
            if (Updated != null)
                Updated(this, EventArgs.Empty);
        }

        private bool IsRelevantContentReference(byte[] contentID)
        {
            if ((DisplayType != DisplayType.ContentReference) && (DisplayType != DisplayType.MediaMetaData))
                return false;

            if ((DisplayType == DisplayType.MediaMetaData) && (Data is MediaMetaData))
                return ((MediaMetaData)Data).HasContentReference(contentID);

            if ((DisplayType == DisplayType.ContentReference) && (Data is ContentReference))
                return ByteArrayHelper.IsEqual(((ContentReference)Data).ContentID, contentID);
            
            return false;
        }

        public static DisplayDataCollection Parse(FieldList source)
        {
            DisplayDataCollection res = new DisplayDataCollection();

            if ((source != null) && (source.Count > 0))
            {
                FieldListNavigator nav = new FieldListNavigator(source);

                if (nav.FindFirst(MessageOutFieldID.SlotDisplayDataTypeID))
                {
                    while (nav.CurrentID == (short)MessageOutFieldID.SlotDisplayDataTypeID)
                    {
                        DisplayType dt = (DisplayType)(nav.Current.AsByte() ?? (byte)DisplayType.Null);
                        object data = null;
                        short? signpost = null;

                        if ((nav.Next != null) && (nav.Next.FieldID == (short)MessageOutFieldID.SlotDisplayData))
                        {
                            switch (dt)
                            {
                                case DisplayType.Null:
                                    data = null;
                                    break;

                                case DisplayType.String:
                                    data = nav.Next.AsString() ?? String.Empty;
                                    break;

                                case DisplayType.Integer:
                                    data = nav.Next.AsNumber() ?? 0;
                                    break;

                                case DisplayType.ContentReference:
                                    {
                                        FieldList fl = nav.Next as FieldList;

                                        if (fl != null)
                                            data = new ContentReference(fl);

                                        break;
                                    }

                                case DisplayType.MediaMetaData:
                                    {
                                        FieldList fl = nav.Next as FieldList;

                                        if (fl != null)
                                            data = new MediaMetaData(fl);

                                        break;
                                    }
                            }
                        }

                        // signpost
                        if ((nav.NextNext != null) && (nav.NextNext.FieldID == (short)NaviAgentFieldID.Signpost))
                        {
                            signpost = nav.NextNext.AsShort() ?? -1;

                            if (signpost < 0)
                                signpost = null;
                        }

                        res.Add(new DisplayData(dt, data, signpost));

                        if (!nav.FindNext(MessageOutFieldID.SlotDisplayDataTypeID))
                            break;
                    }
                }
            }

            return res;
        }

        #region ICacheable implementation

        public CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public void Persist(Stream str)
        {
            str.WriteByte(0);
            str.WriteByte((byte)DisplayType);
            
            // signpost
            str.WriteBool(Signpost.HasValue);
            if (Signpost.HasValue)
                str.WriteShort(Signpost.Value);

            // data
            if (ValidateData(data))
            {
                str.WriteByte(1);

                switch (DisplayType)
                {
                    case DisplayType.String:
                        BinaryHelper.WriteString(str, (string)data);
                        break;

                    case DisplayType.Integer:
                        str.WriteInteger((int)data);
                        break;

                    case DisplayType.ContentReference:
                        ((ContentReference)data).Persist(str);
                        break;

                    case DisplayType.MediaMetaData:
                        ((MediaMetaData)data).Persist(str);
                        break;

                    case DisplayType.Null:
                        break;
                }
            }
            else
                str.WriteByte(0);

            // downloaded data
            CacheHelper.PackToStream(str, DownloadedData);
        }

        public void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
            {
                DisplayType = (DisplayType)str.ReadByte();

                if (str.ReadBool())
                    Signpost = str.ReadShort();
                else
                    Signpost = null;

                if (str.ReadByte() == 1)
                {
                    switch (DisplayType)
                    {
                        case DisplayType.String:
                            data = BinaryHelper.ReadString(str);
                            break;

                        case DisplayType.Integer:
                            data = str.ReadInteger();
                            break;

                        case DisplayType.ContentReference:
                            ContentReference cr = new ContentReference();
                            cr.Restore(str);

                            data = cr;
                            break;

                        case DisplayType.MediaMetaData:
                            MediaMetaData mmd = new MediaMetaData();
                            mmd.Restore(str);

                            data = mmd;
                            break;

                        case DisplayType.Null:
                        default:
                            data = null;
                            break;
                    }
                }
                else
                    data = null;

                DownloadedData = CacheHelper.UnpackFromStream(str);
            }
        }

        #endregion
    }

    public class DisplayDataCollection : IEnumerable<DisplayData>, ICacheable
    {
        public int Count
        {
            get { return data.Count; }
        }

        private List<DisplayData> data = new List<DisplayData>();
        
        public DisplayDataCollection()
        {
        }

        public DisplayData this[int index]
        {
            get
            {
                if ((index >= 0) && (index < data.Count))
                    return data[index];
                else
                    return null;
            }
        }

        public void Add(DisplayData dd)
        {
            data.Add(dd);
        }

        public void Clear()
        {
            data.Clear();
        }

        public IEnumerator<DisplayData> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        public override string ToString()
        {
            return String.Format("Display data collection: items -> {0}", Count);
        }

        #region ICacheable implementation

        public CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public void Persist(Stream str)
        {
            str.WriteByte(0);
            str.WriteInteger(data.Count);

            if (data.Count > 0)
                foreach (DisplayData dd in data)
                    dd.Persist(str);
        }

        public void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
            {
                int dataCount = str.ReadInteger();

                if (dataCount > 0)
                {
                    for (int i = 0; i < dataCount; i++)
                    {
                        DisplayData dd = new DisplayData();
                        dd.Restore(str);

                        data.Add(dd);
                    }
                }
            }
        }

        #endregion
    }
}
