using System;
using System.Collections.Generic;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public class ResidentMediaManager
    {
        private Dictionary<string, Dictionary<DeviceGroup, string>> media = new Dictionary<string, Dictionary<DeviceGroup, string>>();
        
        public ResidentMediaManager()
        {
        }

        public string FindFile(string key, DeviceGroup device)
        {
            Dictionary<DeviceGroup, string> temp;

            if (media.TryGetValue(key, out temp))
            {
                string fileName;

                if (temp.TryGetValue(device, out fileName))
                    return fileName;
            }

            return String.Empty;
        }

        public void Unpack(string source)
        {
            if (!String.IsNullOrEmpty(source))
            {
                string[] parts = source.Split(',');

                for (int i = 0; i < parts.Length; i += 3)
                {
                    if ((i + 2) < parts.Length)
                    {
                        string id = parts[i];
                        string device = parts[i + 1];
                        string fileName = parts[i + 2];

                        if (!String.IsNullOrWhiteSpace(id) && !String.IsNullOrWhiteSpace(fileName))
                        {
                            // check if storage for the id exists
                            if (!media.ContainsKey(id))
                                media[id] = new Dictionary<DeviceGroup, string>();

                            // find device group
                            DeviceGroup dg = SystemAgent.DefaultDeviceGroup;

                            if (!String.IsNullOrWhiteSpace(device))
                            {
                                short dgNum;

                                if (Int16.TryParse(device, out dgNum))
                                    dg = (DeviceGroup)dgNum;
                            }

                            // finally add the filename
                            media[id][dg] = fileName;
                        }
                    }
                }
            }
        }
    }
}
