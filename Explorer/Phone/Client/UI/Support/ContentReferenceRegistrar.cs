using System.Collections.Generic;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;

namespace Wave.UI
{
    public class ContentReferenceRegistrar
    {
        private List<Pair<byte[], DisplayData>> data = new List<Pair<byte[], DisplayData>>();
        
        public void Add(DisplayDataCollection slots)
        {
            if ((slots == null) || (slots.Count == 0))
                return;

            foreach (DisplayData dd in slots)
                Add(dd);
        }

        public void Add(DisplayData dd)
        {
            if (dd == null)
                return;

            switch (dd.DisplayType)
            {
                case DisplayType.ContentReference:
                    {
                        if ((dd.Data is ContentReference) && (((ContentReference)dd.Data).ContentID != null))
                            data.Add(new Pair<byte[], DisplayData>(((ContentReference)dd.Data).ContentID, dd));
                        
                        break;
                    }

                case DisplayType.MediaMetaData:
                    {
                        if (dd.Data is MediaMetaData)
                        {
                            MediaMetaData mmd = dd.Data as MediaMetaData;

                            if (mmd != null)
                                foreach (ContentReference cref in mmd)
                                    data.Add(new Pair<byte[], DisplayData>(cref.ContentID, dd));
                        }
                        
                        break;
                    }
            }
        }

        public void Notify(byte[] contentID, byte[] buffer)
        {
            if ((contentID == null) || (buffer == null))
                return;

            foreach (Pair<byte[], DisplayData> item in data)
            {
                if (ByteArrayHelper.IsEqual(item.First, contentID))
                    item.Second.OnDownloadReady(contentID, buffer);
            }
        }
    }
}
