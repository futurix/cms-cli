using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class MediaMetaData : ICacheable, IEnumerable<ContentReference>
    {
        private List<ContentReference> contentReferences = new List<ContentReference>();

        public MediaMetaData()
        {
        }

        public MediaMetaData(FieldList source)
        {
            Unpack(source);
        }

        public ContentReference this[DeviceGroup device]
        {
            get
            {
                foreach (ContentReference cref in contentReferences)
                    if (cref.DeviceGroup == device)
                        return cref;
                
                return null;
            }
        }

        public bool HasContentReference(byte[] contentID)
        {
            foreach (ContentReference cref in contentReferences)
                if (ByteArrayHelper.IsEqual(cref.ContentID, contentID))
                    return true;

            return false;
        }

        public override string ToString()
        {
            return String.Format("Media metadata: {0} content references", contentReferences.Count);
        }

        private void Unpack(FieldList source)
        {
            if (source != null)
            {
                foreach (IFieldBase field in source)
                {
                    if (field is FieldList)
                        contentReferences.Add(new ContentReference((FieldList)field));
                }
            }
        }

        #region ICacheable implementation

        public CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public void Persist(Stream str)
        {
            str.WriteByte(0);
            str.WriteInteger(contentReferences.Count);

            if (contentReferences.Count > 0)
                foreach (ContentReference cref in contentReferences)
                    cref.Persist(str);
        }

        public void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
            {
                int numberOfCrefs = str.ReadInteger();

                if (numberOfCrefs > 0)
                {
                    for (int i = 0; i < numberOfCrefs; i++)
                    {
                        ContentReference cref = new ContentReference();
                        cref.Restore(str);

                        contentReferences.Add(cref);
                    }
                }
            }
        }

        #endregion

        #region IEnumerable implementation

        public IEnumerator<ContentReference> GetEnumerator()
        {
            return contentReferences.GetEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return contentReferences.GetEnumerator();
        }

        #endregion
    }
}
