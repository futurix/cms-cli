using System;
using System.Web;

namespace Demo
{
    public class Tile : IHttpHandler
    {
        public const string ImageFormat = "http://192.168.20.106/LiveTileDemo/Backgrounds/{0}.jpg";
        
        public bool IsReusable
        {
            get { return false; }
        }
        
        public void ProcessRequest(HttpContext context)
        {
            // prepare data
            Random rnd = new Random();
            int random = rnd.Next(0, 7), image = rnd.Next(0, 6), backImage = rnd.Next(0, 6);

            // output XML
            context.Response.ContentType = "text/xml";
            context.Response.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            context.Response.Write("<LiveTile>");
            context.Response.Write("  <Title>" + DateTime.Now.ToShortTimeString() + "</Title>");
            context.Response.Write("  <Image>" + ((image != 0) ? String.Format(ImageFormat, image) : String.Empty) + "</Image>");
            context.Response.Write("  <Count>" + random.ToString() + "</Count>");
            context.Response.Write("  <BackTitle>" + DateTime.Now.ToLongTimeString() + "</BackTitle>");
            context.Response.Write("  <BackImage>" + ((backImage != 0) ? String.Format(ImageFormat, backImage) : String.Empty) + "</BackImage>");
            context.Response.Write("  <BackText>Example of longer back text</BackText>");
            context.Response.Write("</LiveTile>");
        }
    }
}