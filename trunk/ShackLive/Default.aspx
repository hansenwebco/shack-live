<%@ Page Language="C#" EnableViewState="false" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ShackLive._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>ShackLive</title>
  <link href="css/main.css" rel="stylesheet" type="text/css" />
  <script src="js/jquery-1.3.2.js" type="text/javascript"></script>
  <script src="js/jquery-ui-1.7.1.custom.min.js" type="text/javascript"></script>

  <script type="text/javascript">

    $(document).ready(function() {
    loadJson();
    window.setInterval(loadJson, 30000); // recall load
    });

    var lastJson;
    function loadJson() {
      var flash = false;

      $('#loading').show();
      
      $.getJSON("/json/shackapi_json.aspx",
        function(data) {
          $('#posts').html(''); // reset the html area

          $.each(data, function(i, item) {

            var lastpos = GetLastPosition(item.id)

            
            var display_text = i + 1 + ". " + item.preview + "<br/><span id='info'>by: " + item.author +  " - " + item.age + " - <a target='_blank' href='http://www.shacknews.com/laryn.x?id=" + item.id + "#itemanchor_" + item.id + "'>" + item.replies + " replies</a> [ " + item.ppm.toFixed(4) + " ]</span>"


            if (lastpos >= 0 && lastpos > i)
              $("<div/>").attr("class", "post").attr("flash", flash).css('background-color', 'green').html(display_text + "(+" + (lastpos - i) + ")").effect('highlight', {  }, 2000).appendTo("#posts");
            else if (lastpos >= 0 && lastpos < i)
              $("<div/>").attr("class", "post").attr("flash", flash).css('background-color', 'red').html(display_text + "(-" + (i - lastpos) + ")").effect('highlight', {}, 2000).appendTo("#posts");
            else if ((lastJson != null) && (lastJson[i] != null) && (lastJson[i].replies != null) && (lastJson[i].replies < item.replies))
              $("<div/>").attr("class", "post").attr("flash", flash).css('background-color', '#E9AB17').html(display_text).effect('highlight', {}, 2000).appendTo("#posts");
            else
              $("<div/>").attr("class", "post").attr("flash", flash).html(display_text).appendTo("#posts");

          });

          lastJson = data;
          $('#loading').hide();

        });
      }
      function GetLastPosition(id) {
        
        if (lastJson == null)
          return -2;

        var last = -1;

        $.each(lastJson, function(i, item) {

          if (item.id == id) {
            last = i;
            return false;
          }
        });

        return last;
      }
  </script>
  
  
  
</head>
<body>
  <form id="form1" runat="server">

   <div id="header"><div id="title">ShackLive</div> <div id="loading"><img align="absmiddle" src="images/ajax-loader.gif" /></div></div>
    <div id="posts">
    
    </div>
  
  

      <asp:Repeater ID="RepeaterPosts" runat="server" Visible="false"> 
        <ItemTemplate>
          <div class="post">
            <%#Eval("preview") %> - ( PPM - <%# String.Format("{0:0.000}",Eval("ppm")) %> ) - <a target="_blank" href="http://www.shacknews.com/laryn.x?id=<%#Eval("id") %>#itemanchor_<%#Eval("id") %>"> <%#Eval("Replies") %> replies</a>
          </div>
        </ItemTemplate>
      </asp:Repeater>


  </form>
</body>
</html>
