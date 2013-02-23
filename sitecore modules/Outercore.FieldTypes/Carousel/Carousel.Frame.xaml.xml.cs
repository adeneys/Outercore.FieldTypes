#region using

using System;
using System.IO;
using System.Web.UI;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.HtmlControls.Data;
using Sitecore.Web.UI.XamlSharp.Xaml;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Collections.Specialized;
using Sitecore.Text;
using Sitecore.Data.Fields;
using Sitecore.Resources;
using Sitecore.Web.UI;
using Sitecore.StringExtensions;

#endregion

namespace Outercore.FieldTypes.Carousel {
  public class CarouselFramePage : XamlMainControl {
    #region Controls and properties

    public bool Disabled {
      get { return MainUtil.GetBool(StringUtil.GetString(ViewState["Disabled"]), false); }
      set { ViewState["Disabled"] = value; }
    }

    protected Border Items;
    protected HtmlInputHidden FieldID;

    protected string ItemID {
      get {
        return StringUtil.GetString(ViewState["ItemID"]);
      }
      set {
        ViewState["ItemID"] = value;
      }
    }

    protected NameValueCollection Parameters {
      get { return new UrlString(RawSource).Parameters; }
    }

    public string Source {
      get {
        if (RawSource.Length == 0) {
          return string.Empty;
        }

        return new UrlString(RawSource).Path;
      }
    }

    public string RawSource {
      get { return StringUtil.GetString(ViewState["RawSource"]); }
      set { ViewState["RawSource"] = value; }
    }

    #endregion

    protected override void OnLoad(EventArgs e) {
      base.OnLoad(e);

      ItemID = WebUtil.GetQueryString("id");
      RawSource = WebUtil.GetQueryString("s");
      FieldID.Value = WebUtil.GetQueryString("fid");
      Disabled = MainUtil.GetBool(WebUtil.GetQueryString("d"), false);

      RenderCarousel();
    }

    void RenderCarousel() {
      var currentItem = GetItem();
      if (currentItem == null) {
        Items.InnerHtml = "<strong>Current item not found</strong>";
        return;
      }

      var items = LookupSources.GetItems(GetItem(), Source);
      var output = new HtmlTextWriter(new StringWriter());

      var ul = new TagBuilder("ul");
      if (Disabled) {
        ul.Class = "disabled";
      }

      ul.Start(output);
      RenderItems(output, items);
      output.Write("</ul>");

      Items.InnerHtml = output.InnerWriter.ToString();
    }

    void RenderItems(HtmlTextWriter output, IList<Item> items) {
      foreach (var item in items) {
        RenderItem(output, item);
      }
    }

    void RenderItem(HtmlTextWriter output, Item originalItem) {
      var item = originalItem;
      var fieldName = Parameters["image"];

      if (!string.IsNullOrEmpty(fieldName)) {
        ImageField field = originalItem.Fields[fieldName];
        Assert.IsNotNull(field, "The item {0} does not have an image field '{1}'. Check carousel field source.".FormatWith(originalItem.Paths.ContentPath, fieldName));

        if (field.MediaItem != null) {
          item = field.MediaItem;
        }
      }

      string url;

      if (item.Paths.IsMediaItem) {
        MediaItem media = item;
        var options = new MediaUrlOptions(96, 96, true);
        url = "/sitecore/shell/" + MediaManager.GetMediaUrl(media, options);
      }
      else {
        url = Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id48x48);
      }

      var li = new TagBuilder("li") { ID = originalItem.ID.ToString() };
      if (originalItem.ID.ToString() == WebUtil.GetQueryString("v")) {
        li.Class = "selected";
      }

      li.Start(output);

      var container = new TagBuilder("div") { Class = "image-container" };
      if (!item.Paths.IsMediaItem) {
        container.Class += " small";
      }
      container.Start(output);

      new TagBuilder("img").Add("src", url).ToString(output);

      container.End(output);

      new TagBuilder("span") { Class = "text", InnerHtml = GetText(originalItem)}.ToString(output);

      li.End(output);
    }

    Item GetItem() {
      return Client.ContentDatabase.GetItem(ItemID);
    }

    string GetText(Item item) {
      var fieldName = Parameters["text"];

      if (!string.IsNullOrEmpty(fieldName) && item[fieldName].Length > 0) {
        return item[fieldName];
      }

      return item.DisplayName;
    }
  }
}