using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Xml;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;

namespace ShackLive
{
    public partial class _Default : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {

        }

    }


}

public class ShackPost
{
    public string preview { get; set; }
    public double ppm { get; set; }
    public int replies { get; set; }
    public string id { get; set; }
    public string storyid { get; set; }
    public string author { get; set; }
    public string age { get; set; }
}