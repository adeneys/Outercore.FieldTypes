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
using Sitecore.Web.UI.XamlSharp.Xaml;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using Sitecore.StringExtensions;
using Sitecore.Resources;
using Sitecore.Web.UI;
using System.Collections.Specialized;
using Sitecore.Data.Fields;

#endregion

namespace Outercore.FieldTypes.Carousel
{
    public class VisualListFramePage : XamlMainControl
    {
        #region Controls and properties

        public bool Disabled
        {
            get { return MainUtil.GetBool(StringUtil.GetString(ViewState["Disabled"]), false); }
            set { ViewState["Disabled"] = value; }
        }

        protected Border Items;
        protected HtmlInputHidden ControlID;

        protected string ItemID
        {
            get
            {
                return StringUtil.GetString(ViewState["ItemID"]);
            }
            set
            {
                ViewState["ItemID"] = value;
            }
        }

        protected string Language
        {
            get
            {
                return StringUtil.GetString(ViewState["Language"]);
            }
            set
            {
                ViewState["Language"] = value;
            }
        }
        protected string Version
        {
            get
            {
                return StringUtil.GetString(ViewState["Version"]);
            }
            set
            {
                ViewState["Version"] = value;
            }
        }

        protected string FieldID
        {
            get
            {
                return StringUtil.GetString(ViewState["FieldID"]);
            }
            set
            {
                ViewState["FieldID"] = value;
            }
        }

        protected NameValueCollection Parameters
        {
            get { return new UrlString(RawSource).Parameters; }
        }

        protected string RawSource
        {
            get
            {
                return StringUtil.GetString(ViewState["RawSource"]);
            }
            set
            {
                ViewState["RawSource"] = value;
            }
        }

        protected string Source
        {
            get
            {
                if (RawSource.Length == 0)
                {
                    return string.Empty;
                }

                return new UrlString(RawSource).Path;
            }
        }


        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (AjaxScriptManager.IsEvent)
            {
                return;
            }

            ItemID = WebUtil.GetQueryString("id");
            FieldID = WebUtil.GetQueryString("fid");
            Language = WebUtil.GetQueryString("lang");
            this.Version = WebUtil.GetQueryString("ver");
            RawSource = WebUtil.GetQueryString("s");
            ControlID.Value = WebUtil.GetQueryString("cid");
            Disabled = MainUtil.GetBool(WebUtil.GetQueryString("d"), false);

            RenderItems();
        }

        public void AddItem(string id)
        {
            MediaItem item = Client.GetItemNotNull(id);
            var html = RenderItem(item, false);

            SheerResponse.Eval("scVisualList.appendItem('{0}', '{1}');".FormatWith(html, id));
        }

        void RenderItems()
        {
            var items = GetItems();
            var output = new HtmlTextWriter(new StringWriter());

            output.Write("\r\n");
            var ul = new TagBuilder("ul") { ID = "list" };
            if (Disabled)
            {
                ul.Class = "disabled";
            }

            ul.Start(output);
            output.Write("\r\n");

            RenderItems(output, items);

            output.Write("</ul>");

            Items.InnerHtml = output.InnerWriter.ToString();
        }

        void RenderItems(HtmlTextWriter output, IList<Item> items)
        {
            foreach (var item in items)
            {
                output.Write(RenderItem(item, true));
                output.Write("\r\n");
            }
        }

        string RenderItem(Item originalItem, bool includeTop)
        {
            Item item = originalItem;
            string fieldName = Parameters["image"];

            if (!string.IsNullOrEmpty(fieldName))
            {
                ImageField field = originalItem.Fields[fieldName];
                Assert.IsNotNull(field, "The item {0} does not have an image field '{1}'. Check visual list field source.".FormatWith(originalItem.Paths.ContentPath, fieldName));

                if (field.MediaItem != null)
                {
                    item = field.MediaItem;
                }
            }

            var output = new HtmlTextWriter(new StringWriter());
            string url;

            if (item.Paths.IsMediaItem)
            {
                MediaItem media = item;

                // Look for image width (w) & height (h) parameters
                int iWidth = 96;
                int iHeight = 96;

                if (!String.IsNullOrEmpty(Parameters["w"]))
                    iWidth = int.Parse(Parameters["w"]);
                if (!String.IsNullOrEmpty(Parameters["h"]))
                    iHeight = int.Parse(Parameters["h"]);

                var options = new MediaUrlOptions(iWidth, iHeight, true);

                url = "/sitecore/shell/" + MediaManager.GetMediaUrl(media, options);
            }
            else
            {
                url = Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id48x48);
            }

            TagBuilder li = null;
            if (includeTop)
            {
                li = new TagBuilder("li") { ID = originalItem.ID.ToString() }.Start(output);
            }

            var container = new TagBuilder("div") { Class = "image-container" };
            if (!item.Paths.IsMediaItem)
            {
                container.Class += " small";
            }
            container.Start(output);

            new TagBuilder("img").Add("src", url).Add("align", "middle").ToString(output);

            container.End(output);

            new TagBuilder("span") { Class = "text", InnerHtml = GetText(originalItem) }.ToString(output);

            output.Write("<div class=\"delete\"></div>");

            if (includeTop)
            {
                li.End(output);
            }

            return output.InnerWriter.ToString();
        }

        IList<Item> GetItems()
        {
            var result = new List<Item>();
            var values = new ListString(GetItem()[FieldID]);

            foreach (var id in values)
            {
                var item = Client.ContentDatabase.GetItem(id);
                if (item != null)
                {
                    result.Add(item);
                }
                //if id is not a valid Sitecore id and has at least one closing angle bracket
		else if(!Sitecore.Data.ID.IsID(id) && id.IndexOf(">", System.StringComparison.Ordinal) > -1 )
                {
                    // If this field used to be an Image field then the value was stored 
                    // as an XmlValue instead of just an ItemID
                    // So, we'll try to parse it out.
                    Sitecore.Shell.Applications.ContentEditor.XmlValue xmlVal = new Sitecore.Shell.Applications.ContentEditor.XmlValue(id, "image");
                    string mediaID = xmlVal.GetAttribute("mediaid");
                    if (!String.IsNullOrEmpty(mediaID))
                    {
                        item = Client.ContentDatabase.GetItem(mediaID);
                        if (item != null) result.Add(item);
                    }
                }
            }

            return result;
        }

        Item GetItem()
        {
            return Assert.ResultNotNull(Client.ContentDatabase.GetItem(ItemID, Sitecore.Globalization.Language.Parse(this.Language), Sitecore.Data.Version.Parse(this.Version)), "current item");
        }

        string GetText(Item item)
        {
            var fieldName = Parameters["text"];

            if (!string.IsNullOrEmpty(fieldName) && item[fieldName].Length > 0)
            {
                return item[fieldName];
            }

            return item.DisplayName;
        }
    }
}