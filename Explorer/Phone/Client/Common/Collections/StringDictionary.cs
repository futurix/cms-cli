using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Wave.Common
{
    [XmlRoot("StringDictionary")]
    public class StringDictionary : Dictionary<string, string>, IXmlSerializable
    {
        #region XML Serialization support

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.HasAttributes)
                {
                    string key = reader["Key"];
                    string value = reader["Value"] ?? String.Empty;

                    if (!String.IsNullOrEmpty(key))
                        Add(key, value);
                }

                reader.Read();
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (string key in Keys)
            {
                writer.WriteStartElement("Item");

                writer.WriteAttributeString("Key", key);
                writer.WriteAttributeString("Value", this[key]);

                writer.WriteEndElement();
            }
        }

        #endregion

        public bool ValueEquals(string key, string newValue)
        {
            string temp = null;

            if (TryGetValue(key, out temp) && (temp == newValue))
                return true;

            return false;
        }

        public void Merge(StringDictionary source)
        {
            if ((source == null) || (source.Count == 0))
                return;

            foreach (var item in source)
                this[item.Key] = item.Value;
        }
    }
}
