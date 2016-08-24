using System.Collections.Generic;
using System.Xml.Serialization;
using Wave.Common;

namespace BuildHelper
{
    [XmlRoot("Answers")]
    public class Answers
    {
        public StringDictionary Substitutions = new StringDictionary();
        
        [XmlArray("PlatformOptions")]
        [XmlArrayItem("Option")]
        public List<string> PlatformOptions = new List<string>();

        public StringDictionary ClientOptions = new StringDictionary();
    }
}
