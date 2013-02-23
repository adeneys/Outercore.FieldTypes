#region using

using System;
using System.Web.UI;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web;
using Sitecore.Text;
using Sitecore.StringExtensions;
using Sitecore.Web.UI.Sheer;
using Sitecore.Shell.Applications.Dialogs.ItemLister;
using Sitecore.Shell.Applications.Dialogs.MediaBrowser;
using Sitecore.Diagnostics;

#endregion

namespace Outercore.FieldTypes {
  public class VisualListContentField : Border, IContentField {

    public new bool Disabled {
      get { return MainUtil.GetBool(StringUtil.GetString(ViewState["Disabled"]), false); }
      set { ViewState["Disabled"] = value; }
    }

    public string ItemID { get; set; }
    public string FieldID { get; set; }
    public string ItemLanguage { get; set; }
    public string ItemVersion { get; set; }
    
    public string Source {
      get { return StringUtil.GetString(ViewState["Source"]); }
      set { ViewState["Source"] = value; }
    }

    public string LastSelectedItemID {
      get { return StringUtil.GetString(ViewState["LastSelectedItemID"]); }
      set { ViewState["LastSelectedItemID"] = value; }
    }

    public new string Value { get; set; }

    public string RealValue
    {
        get
        {
            return base.GetViewStateString("RealValue");
        }
        set
        {
            Assert.ArgumentNotNull(value, "value");
            base.SetViewStateString("RealValue", value);
        }
    }


    protected void SetModified()
    {
        //base.SetModified();
        //if (base.TrackModified)
        //{
            Sitecore.Context.ClientPage.Modified = true;
        //}
    }


    public override void HandleMessage(Message message) {

      if (message["id"] == this.ID) {
        if (message.Name.StartsWith("visuallist") && Disabled) {
          return;
        }

        if (message.Name == "visuallist:additem") {
          Sitecore.Context.ClientPage.Start(this, "AddItem");
          return;
        }

        if (message.Name == "visuallist:clear") {
          Sitecore.Context.ClientPage.Start(this, "DoClear");
          return;
        }
      }

      base.HandleMessage(message);
    }

    protected override void DoRender(HtmlTextWriter output) {
      var src = new UrlString("/sitecore/shell/~/xaml/Outercore.FieldTypes.VisualList.Frame.aspx");
      src.Append("s", Source);
      src.Append("id", ItemID);
      src.Append("cid", ID);
      src.Append("fid", FieldID);
      src.Append("lang", ItemLanguage);
      src.Append("ver", ItemVersion);
      if (Disabled)
      {
        src.Append("d", "1");
      }

      output.Write("<div id='{0}_pane' class='scContentControl scVisualList'>".FormatWith(ID));
      output.Write("<iframe id='{0}_frame' name='{0}_frame' src='{1}' frameborder='0' marginwidth='0' marginheight='0' width='100%' height='264' allowtransparency='allowtransparency'></iframe>".FormatWith(ID, src.ToString()));
      output.Write("</div>");

      output.Write("<input type='hidden' id='{0}' value='{1}' />".FormatWith(ID + "_selected", RealValue));
    }

    protected void AddItem(ClientPipelineArgs args) {
      if (args.IsPostBack) {
        if (args.HasResult) {
          SetModified();
          LastSelectedItemID = args.Result;
          SheerResponse.Eval("window.frames['{0}_frame'].window.scVisualList.addItem('{1}');".FormatWith(ID, args.Result));
        }
      }
      else {
        var source = new UrlString(StringUtil.GetString(Source, "/sitecore/media library")).Path;

        Item lastSelectedItem = null;
        if (!string.IsNullOrEmpty(LastSelectedItemID)) {
          lastSelectedItem = Client.ContentDatabase.GetItem(LastSelectedItemID);
        }

        if (source.Contains("/sitecore/media library")) {
          var options = new MediaBrowserOptions();
          
          if (source.StartsWith("~")) {
            options.Root = Client.GetItemNotNull(ItemIDs.MediaLibraryRoot);
            options.SelectedItem = Client.GetItemNotNull(source.Substring(1));
          }
          else {
            options.Root = Client.GetItemNotNull(source);
          }

          if (lastSelectedItem != null && lastSelectedItem.Parent.Paths.IsDescendantOf(options.Root)) {
            options.SelectedItem = lastSelectedItem.Parent;
          }

          SheerResponse.ShowModalDialog(options.ToUrlString().ToString(), true);
        }
        else {
          var options = new SelectItemOptions { Title = "Please Select an Item", Text = "Select an item to add", Icon = "Applications/32x32/star_green.png" };
          if (source.StartsWith("~")) {
            options.Root = Client.GetItemNotNull(ItemIDs.ContentRoot);
            options.SelectedItem = Client.GetItemNotNull(source.Substring(1));
          }
          else {
            options.Root = Client.GetItemNotNull(source);
          }

          if (lastSelectedItem != null && lastSelectedItem.Paths.IsDescendantOf(options.Root)) {
            options.SelectedItem = lastSelectedItem;
          }

          SheerResponse.ShowModalDialog(options.ToUrlString().ToString(), true);
        }

        args.WaitForPostBack();
      }
    }

    protected void DoClear(ClientPipelineArgs args) {
      if (args.IsPostBack) {
        if (args.Result != "no" && args.Result != "undefined") {
            SetModified();
          SheerResponse.Eval("window.frames['{0}_frame'].window.scVisualList.clear();".FormatWith(ID));
        }
      }
      else {
        SheerResponse.Confirm("Are you sure you want to remove all items?");
        args.WaitForPostBack();
      }
    }

    #region IContentField Members

    public string GetValue()
    {
        var selectedFormValue = System.Web.HttpContext.Current.Request.Form[ID + "_selected"];

        if (selectedFormValue == null)
        {
            return this.RealValue;
        }
        return selectedFormValue;
    }

    public void SetValue(string value) {
      RealValue = value;
    }

    #endregion
  }
}