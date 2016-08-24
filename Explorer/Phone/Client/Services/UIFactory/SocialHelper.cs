using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Microsoft.Phone.Tasks;
using Wave.Common;
using Wave.Platform;

namespace Wave.Services
{
    public static class SocialHelper
    {
        public static void PublishSocial(SocialNetwork target, string data)
        {
            switch (target)
            {
                case SocialNetwork.Facebook:
                    {
                        FacebookData fb = ParseFacebookJson(data);

                        if (fb != null)
                        {
                            if (fb.Link == null)
                            {
                                if (fb.Title != null)
                                    PublishSocialStatus(String.Format("{0}\n{1}", fb.Title, fb.Text ?? String.Empty));
                                else
                                    PublishSocialStatus(fb.Text ?? String.Empty);
                            }
                            else
                                PublishSocialLink(fb.Title, fb.Text, fb.Link);
                        }
                        
                        break;
                    }

                case SocialNetwork.Twitter:
                    PublishSocialStatus(data);
                    break;
            }
        }
        
        public static void PublishSocialStatus(string message)
        {
            if (message != null)
            {
                ShareStatusTask share = new ShareStatusTask();
                share.Status = message;

                share.Show();
            }
        }

        public static void PublishSocialLink(string title, string message, string uri)
        {
            if (!String.IsNullOrEmpty(title) && !String.IsNullOrEmpty(uri))
            {
                ShareLinkTask share = new ShareLinkTask();
                share.Title = title;
                share.Message = message ?? String.Empty;
                share.LinkUri = new Uri(uri, UriKind.Absolute);

                share.Show();
            }
        }

        private static FacebookData ParseFacebookJson(string data)
        {
            if (data != null)
            {
                byte[] rawData = StringHelper.GetBytes(data);

                if (rawData != null)
                {
                    using (MemoryStream ms = new MemoryStream(rawData))
                    {
                        DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(FacebookData));

                        return js.ReadObject(ms) as FacebookData;
                    }
                }
            }

            return null;
        }

        [DataContract]
        public class FacebookData
        {
            [DataMember(Name = "name")]
            public string Title { get; set; }

            [DataMember(Name = "href")]
            public string Link { get; set; }

            [DataMember(Name = "caption")]
            public string Subtitle { get; set; }

            [DataMember(Name = "description")]
            public string Text { get; set; }
        }
    }
}
