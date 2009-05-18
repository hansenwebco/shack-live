using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace ShackLive
{
    public partial class Pics : System.Web.UI.Page
    {

        List<ImageUrls> urls = new List<ImageUrls>();

        protected void Page_Load(object sender, EventArgs e)
        {
            int storyID = int.Parse(Request.QueryString["story"]);
            LoadPosts(null, storyID.ToString(), null);

            RepeaterPics.DataSource = urls.ToList();
            RepeaterPics.DataBind();

            WebClient wc = new WebClient();
            foreach (var item in urls)
            {
                try
                {

                    // wc.DownloadFile(item.url, "c:\\temp\\crawl\\" + Path.GetFileName(item.url));
                }
                catch
                {


                }

            }



        }

        protected void LoadPosts(int? pageid, string story, string threadID)
        {
            string sUrl = "";


            if (pageid != null && story != null)
                sUrl = ConfigurationManager.AppSettings["siteAPIURL"] + story + "." + pageid + ".xml";
            else if (pageid != null && threadID != null)
                sUrl = ConfigurationManager.AppSettings["siteAPIURL"] + "thread/" + threadID + ".xml";
            else
                sUrl = ConfigurationManager.AppSettings["siteAPIURL"] + story + ".1.xml";

            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            XmlDocument doc = new XmlDocument();
            doc.Load(sUrl); // load the document with the writer
            doc.WriteTo(writer);

            XmlNode node = doc.SelectSingleNode("comments");

            int pages = 1;
            if (node.Attributes["last_page"].Value.Length > 0)
                pages = Convert.ToInt32(node.Attributes["last_page"].Value);

            XmlNodeList nodes;
            if (threadID == null)
                nodes = doc.SelectNodes("comments/comment");
            else
                nodes = doc.GetElementsByTagName("comment");

            foreach (XmlNode item in nodes)
            {

                //#(http[s]?//:([0-9a-z_. ]/)*[0-9a-z _.](bmp|gif|jpg|png)+)#i

                string post = item.FirstChild.InnerText.ToString().Trim();
                Regex regex = new Regex(@"http://[\w/:.]+\.jpg");
                MatchCollection matches = regex.Matches(post);

                foreach (var match in matches)
                {
                    ImageUrls url = new ImageUrls();
                    url.id = item.Attributes["id"].Value.Trim();
                    url.poster = item.Attributes["author"].Value.Trim();
                    url.url = match.ToString();
                    urls.Add(url);
                }

                if (Convert.ToInt32(item.Attributes["reply_count"].Value) > 0 && threadID == null)
                    LoadPosts(1, null, item.Attributes["id"].Value.Trim());
            }

            stream = null;
            writer = null;
            doc = null;

            if ((pages > 1 && pageid == null || pages > 1 && pageid < pages))
                if (pageid == null)
                    LoadPosts(1, story.ToString(), null);
                else
                    LoadPosts(pageid + 1, story.ToString(), null);

        }
    }
}
public class ImageUrls
{
    public string id { get; set; }
    public string url { get; set; }
    public string poster { get; set; }
}
