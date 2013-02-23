#region using

using System;
using System.Collections.Generic;
using System.Web.UI;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.StringExtensions;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.HtmlControls.Data;

#endregion

namespace Outercore.FieldTypes {
  /// <summary>
  /// Defines the text list content field class.
  /// </summary>
  public class TextListContentField : Sitecore.Web.UI.HtmlControls.Control, IContentField {
    #region Properties

    public new bool Disabled {
      get { return MainUtil.GetBool(StringUtil.GetString(ViewState["Disabled"]), false); }
      set { ViewState["Disabled"] = value; }
    }
    public string ItemID { get; set; }
    public string Source { get; set; }
    public new string Value { get; set; }

    #endregion

    #region Protected methods

    /// <summary>
    /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"></see> object, which writes the content to be rendered on the client.
    /// </summary>
    /// <param name="output">The <see cref="T:System.Web.UI.HtmlTextWriter"></see> object that receives the server control content.</param>
    protected override void DoRender(HtmlTextWriter output) {
      Item current = Client.GetItemNotNull(ItemID, Sitecore.Context.ContentDatabase);
      IList<Item> items = GetItems(current, Source);

      var list = new TagBuilder("div") {ID = ID, Class = "scTextlist"};
      if(Disabled) {
        list.Add("disabled", "true");
      }

      list.Start(output);

      output.Write("<div id='{0}_list' class='textlist-list'>".FormatWith(ID));

      RenderValue(output);

      output.Write("</div>");

      output.Write("<div class='textlist-autocomplete' style='display:none'></div>");

      output.Write("<div class='textlist-choices' style='display:none'>");

      foreach(Item item in items) {
        output.Write("<span sc_text=\"{0}\" sc_value='{1}'>{0}</span>".FormatWith(item.DisplayName, item.ID));
      }

      output.Write("</div>");
      output.Write("</div>");

      output.Write("<input type='hidden' id='{0}' value='{1}' />".FormatWith(ID + "_value", Value));

      output.Write("<link rel=Stylesheet type='text/css' href='/sitecore%20modules/outercore.fieldtypes/textlist/js/textlist.css' />");

      string script = "new Sitecore.FieldTypes.TextList('{0}');".FormatWith(ID);
      script = "<script type='text/javascript' language='javascript'>" + script + "</script>";
      output.Write(script);
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <param name="current">The current.</param>
    /// <param name="source">The source.</param>
    /// <returns>The items.</returns>
    static IList<Item> GetItems(Item current, string source) {
      Assert.ArgumentNotNull(current, "source");
      Assert.ArgumentNotNull(source, "source");

      if (source.Length == 0) {
        return new List<Item>();
      }

      var urlString = new UrlString(source);

      var path = urlString.Path;

      return LookupSources.GetItems(current, path);
    }

    /// <summary>
    /// If the field has value, renders selected items
    /// </summary>
    /// <param name="output">The writer.</param>
    void RenderValue(HtmlTextWriter output) {
      if (string.IsNullOrEmpty(Value)) {
        return;
      }

      var list = new ListString(Value);
      foreach(string id in list) {
        Item item = Sitecore.Context.ContentDatabase.GetItem(id);
        string text = item != null ? item.DisplayName : id;

        output.Write("<div class='textlist-item' sc_value='{0}'>".FormatWith(id));
        output.Write(text);
        output.Write("<a href='#' class='textlist-closebutton'></a>");
        output.Write("</div>");
      }
    }

    #endregion

    #region IContentField Members

    public string GetValue() {
      return WebUtil.GetFormValue(ID + "_value");
    }

    public void SetValue(string value) {
      Value = value;
    }

    #endregion
  }
}