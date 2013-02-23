#region using

using System;
using System.Web.UI;
using Sitecore;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web;
using Sitecore.Text;
using Sitecore.StringExtensions;

#endregion

namespace Outercore.FieldTypes {
  public class CarouselContentField : Border, IContentField {

    public new bool Disabled {
      get { return MainUtil.GetBool(StringUtil.GetString(ViewState["Disabled"]), false); }
      set { ViewState["Disabled"] = value; }
    }

    public string ItemID { get; set; }
    public string Source { get; set; }
    public new string Value { get; set; }
    
    protected override void DoRender(HtmlTextWriter output) {
      var src = new UrlString("/sitecore/shell/~/xaml/Outercore.FieldTypes.Carousel.Frame.aspx");
      src.Append("s", Source);
      src.Append("id", ItemID);
      src.Append("fid", ID);
      src.Append("v", Value);
      if (Disabled) {
        src.Append("d", "1");
      }

      output.Write("<div id='{0}_pane' class='scContentControl scImageList'>".FormatWith(ID));
      output.Write("<iframe id='{0}_frame' src='{1}' frameborder='0' marginwidth='0' marginheight='0' width='100%' height='128' allowtransparency='allowtransparency'></iframe>".FormatWith(ID, src.ToString()));
      output.Write("</div>");

      output.Write("<input type='hidden' id='{0}' value='{1}' />".FormatWith(ID + "_selected", Value));
    }

    #region IContentField Members

    public string GetValue() {
      var selected = WebUtil.GetFormValue(ID + "_selected");
      return selected;
    }

    public void SetValue(string value) {
      Value = value;
    }

    #endregion
  }
}