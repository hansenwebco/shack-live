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
using System.Web.Script.Serialization;

namespace ShackLive.json
{
    public partial class shackapi_json : System.Web.UI.Page
    {


        int pages = 1;
        int storyID = 0;
        List<ShackPost> posts = new List<ShackPost>();
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Expires = -1337;
            Response.ContentType = "application/json";



            if (Application["posts"] != null)
                posts = (List<ShackPost>)Application["posts"];

            LoadPosts(null, null);

            Application["posts"] = posts;

            JavaScriptSerializer js = new JavaScriptSerializer();
            string json = js.Serialize(posts.Where(w => w.replies > 0).OrderByDescending(o => o.ppm).Take(50));

            Response.Write(json);

        }

        protected void LoadPosts(int? pageid, string story)
        {
            string sUrl = "";

            if (pageid != null && story != null)
                sUrl = ConfigurationManager.AppSettings["siteAPIURL"] + story + "." + pageid + ".xml";
            else
                sUrl = ConfigurationManager.AppSettings["siteAPIURL"];

            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            XmlDocument doc = new XmlDocument();
            doc.Load(sUrl); // load the document with the writer
            doc.WriteTo(writer);

            XmlNode node = doc.SelectSingleNode("comments");
            storyID = Convert.ToInt32(node.Attributes["story_id"].Value);

            posts.RemoveAll(obj => obj.storyid != storyID.ToString());



            if (node.Attributes["last_page"].Value.Length > 0)
                pages = Convert.ToInt32(node.Attributes["last_page"].Value);

            foreach (XmlNode item in doc.SelectNodes("comments/comment"))
            {
                string id = item.Attributes["id"].Value.Trim();

                ShackPost sp = new ShackPost();
                bool newpost = false;
                sp = posts.Where(w => w.id == id).FirstOrDefault();
                if (sp == null)
                {
                    sp = new ShackPost();
                    newpost = true;
                }


                sp.ppm = GetPostsPerMinute(Convert.ToInt32(item.Attributes["reply_count"].Value), item.Attributes["date"].Value);
                sp.preview = item.Attributes["preview"].Value.Trim();
                sp.replies = Convert.ToInt32(item.Attributes["reply_count"].Value);
                sp.id = item.Attributes["id"].Value.Trim();
                sp.storyid = storyID.ToString();
                sp.author = item.Attributes["author"].Value.Trim();

                DateTime nodedate;
                string startdate = item.Attributes["date"].Value.Trim();
                DateTime.TryParseExact(startdate.ToString().Substring(0, startdate.ToString().Length - 4), "MMM dd, yyyy h:mmtt", null, System.Globalization.DateTimeStyles.None, out nodedate);

                TimeSpan span = DateTime.Now.AddHours(-1).Subtract(nodedate);


                sp.age = span.Hours + "h " + span.Minutes + "m";
                

                if (newpost == true)
                    posts.Add(sp);
            }
            stream = null;
            writer = null;
            doc = null;

            if ((pages > 1 && pageid == null || pages > 1 && pageid < pages) && Application["posts"] == null)
                if (pageid == null)
                    LoadPosts(1, storyID.ToString());
                else
                    LoadPosts(pageid + 1, storyID.ToString());

        }
        protected double GetPostsPerMinute(int replies, string startdate)
        {
            DateTime nodedate;
            DateTime.TryParseExact(startdate.ToString().Substring(0, startdate.ToString().Length - 4), "MMM dd, yyyy h:mmtt", null, System.Globalization.DateTimeStyles.None, out nodedate);

            TimeSpan span = DateTime.Now.AddHours(-1).Subtract(nodedate);


            return (replies / span.TotalMinutes);

        }

    }
}
