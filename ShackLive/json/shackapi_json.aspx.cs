﻿using System;
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

            if (string.IsNullOrEmpty(Request.QueryString["story"]))
                LoadPosts(null, null);
            else
                LoadPosts(null, Request.QueryString["story"]);


            Application["posts"] = posts;

            JavaScriptSerializer js = new JavaScriptSerializer();
            string json = js.Serialize(posts.Where(w => w.replies > 0).OrderBy(o => o.ppm).Take(50));

            Response.Write(json);

        }

        protected void LoadPosts(int? pageid, string story)
        {
            string sUrl = "";

            if (pageid != null && story != null)
                sUrl = ConfigurationManager.AppSettings["siteAPIURL"] + story + "." + pageid + ".xml";
            else if (pageid == null && story != null)
                sUrl = ConfigurationManager.AppSettings["siteAPIURL"] + story + ".1.xml";
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



                sp.preview = item.Attributes["preview"].Value.Trim();
                sp.replies = Convert.ToInt32(item.Attributes["reply_count"].Value);
                sp.id = item.Attributes["id"].Value.Trim();
                sp.storyid = storyID.ToString();
                sp.author = item.Attributes["author"].Value.Trim();
                sp.date = item.Attributes["date"].Value.Trim();


                if (newpost == true)
                    posts.Add(sp);
            }

            stream = null;
            writer = null;
            doc = null;

            foreach (var post in posts)
            {
                DateTime nodedate;
                string startdate = post.date;
                DateTime.TryParseExact(startdate.ToString(), "ddd MMM dd HH:mm:00 -0800 yyyy", null, System.Globalization.DateTimeStyles.None, out nodedate);
                TimeSpan span = DateTime.Now.AddHours(-3).Subtract(nodedate);
                post.age = span.Hours + "h " + span.Minutes + "m";

                post.ppm = GetPostsPerMinute(post.replies, post.date);

            }

            if ((pages > 1 && pageid == null || pages > 1 && pageid < pages) && Application["posts"] == null)
                if (pageid == null)
                    LoadPosts(1, storyID.ToString());
                else
                    LoadPosts(pageid + 1, storyID.ToString());

        }
        protected double GetPostsPerMinute(int replies, string startdate)
        {
            DateTime nodedate;
            DateTime.TryParseExact(startdate.ToString(), "ddd MMM dd HH:mm:00 -0800 yyyy", null, System.Globalization.DateTimeStyles.None, out nodedate);

            TimeSpan span = DateTime.Now.AddHours(-3).Subtract(nodedate);


            return (span.TotalMinutes / replies);

        }

    }
}
