using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Microsoft.Phone.Shell;

namespace Wave.Agent
{
    public class LiveTileUpdater
    {
        public const string BackgroundFormat = "Shared\\ShellContent\\{0}";
        
        public string URL { get; private set; }
        
        public string Login { get; private set; }
        public string Password { get; private set; }

        private Action completionCallback = null;
        
        public LiveTileUpdater(string url, string login, string password)
        {
            URL = url;
            Login = login;
            Password = password;
        }

        public void StartUpdate(Action callback)
        {
            if (!String.IsNullOrWhiteSpace(URL))
            {
                // remember callback
                completionCallback = callback;

                // start HTTP request
                HttpWebRequest request = HttpWebRequest.Create(URL) as HttpWebRequest;
                request.Method = "GET";

                if (!String.IsNullOrWhiteSpace(Login))
                    request.Headers["Wave-Login"] = Login;

                if (!String.IsNullOrWhiteSpace(Password))
                    request.Headers["Wave-Password"] = Password;

                request.BeginGetResponse(ResponseCallback, request);
            }
            else
                ExecuteCallback();
        }

        private void EndUpdate(string serverData)
        {
            LiveTileData data = ParseXML(serverData);
            UpdateTile(data);

            ExecuteCallback();
        }

        private void ResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest rq = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)rq.EndGetResponse(ar);

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    EndUpdate(reader.ReadToEnd());
            }
            catch
            {
                ExecuteCallback();
            }
        }

        private LiveTileData ParseXML(string input)
        {
            if (!String.IsNullOrWhiteSpace(input))
            {
                XDocument doc = XDocument.Parse(input);

                if (doc != null)
                {
                    XElement root = doc.Element("LiveTile");

                    if (root != null)
                    {
                        LiveTileData res = new LiveTileData();
                        
                        // tile title
                        XElement title = root.Element("Title");

                        if (title != null)
                            res.Title = title.Value;

                        // tile background
                        XElement image = root.Element("Image");

                        if (image != null)
                            res.Image = image.Value;

                        // tile count
                        XElement count = root.Element("Count");

                        if (count != null)
                            res.Count = count.Value;

                        // tile back title
                        XElement backTitle = root.Element("BackTitle");

                        if (backTitle != null)
                            res.BackTitle = backTitle.Value;

                        // tile back background
                        XElement backImage = root.Element("BackImage");

                        if (backImage != null)
                            res.BackImage = backImage.Value;

                        // tile back text
                        XElement backText = root.Element("BackText");

                        if (backText != null)
                            res.BackText = backText.Value;

                        return res;
                    }
                }
            }

            return null;
        }

        private void UpdateTile(LiveTileData data)
        {
            if (data != null)
            {
                ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault();

                if (tile != null)
                {
                    StandardTileData updatedTileData = new StandardTileData();

                    updatedTileData.Title = data.Title;

                    if (!String.IsNullOrWhiteSpace(data.Count))
                    {
                        int count = 0;

                        Int32.TryParse(data.Count, out count);

                        updatedTileData.Count = count;
                    }
                    else
                        updatedTileData.Count = 0;

                    if (!String.IsNullOrWhiteSpace(data.Image))
                    {
                        if (data.Image.StartsWith("http://"))
                            updatedTileData.BackgroundImage = new Uri(data.Image);
                        else
                            updatedTileData.BackgroundImage = new Uri(String.Format(BackgroundFormat, data.Image));
                    }
                    else
                        updatedTileData.BackgroundImage = null;

                    updatedTileData.BackTitle = data.BackTitle;

                    if (!String.IsNullOrWhiteSpace(data.BackImage))
                    {
                        if (data.BackImage.StartsWith("http://"))
                            updatedTileData.BackBackgroundImage = new Uri(data.BackImage);
                        else
                            updatedTileData.BackBackgroundImage = new Uri(String.Format(BackgroundFormat, data.BackImage));
                    }
                    else
                        updatedTileData.BackBackgroundImage = null;

                    updatedTileData.BackContent = data.BackText;

                    tile.Update(updatedTileData);
                }
            }
        }

        private void ExecuteCallback()
        {
            if (completionCallback != null)
                completionCallback();
        }

        private class LiveTileData
        {
            public string Title { get; set; }
            public string Count { get; set; }
            public string Image { get; set; }
            public string BackTitle { get; set; }
            public string BackText { get; set; }
            public string BackImage { get; set; }
        }
    }
}
