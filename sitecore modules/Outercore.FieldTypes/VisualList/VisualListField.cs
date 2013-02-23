using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Outercore.FieldTypes.VisualList
{
    public class VisualListField
    {
        private MultilistField _field = null;

        private MultilistField vlField { get { return _field; } }

        public VisualListField(Field f)
        {
            _field = f;
        }

        public static implicit operator VisualListField(Field f)
        {
            if (f != null)
            {
                return new VisualListField(f);
            }
            return null;
        }

        /// <summary>
        /// For a VisualList, the value contents 
        /// should be a pipe delimited list of MediaItemIDs, but handle old 
        /// Image XmlValue contents as well. (in the case that this field was originally an Image field)
        /// </summary>
        /// <param name="i"></param>
        /// <param name="bRandom"></param>
        /// <returns></returns>
        public List<MediaItem> MediaItems()
        {
            List<MediaItem> results = new List<MediaItem>();

            foreach (string id in vlField.Items)
            {
                Item item = Sitecore.Context.Database.GetItem(id);
                if (item != null)
                {
                    results.Add((MediaItem)item);
                }
                else
                {
                    // If this field used to be an Image field then the value was stored 
                    // as an XmlValue instead of just an ItemID
                    // So, we'll try to parse it out.
                    Sitecore.Shell.Applications.ContentEditor.XmlValue xmlVal = new Sitecore.Shell.Applications.ContentEditor.XmlValue(id, "image");
                    string mediaID = xmlVal.GetAttribute("mediaid");
                    if (!String.IsNullOrEmpty(mediaID))
                    {
                        item = Sitecore.Context.Database.GetItem(mediaID);
                        if (item != null)
                            results.Add((MediaItem)item);
                    }
                }
            }

            return results;
        }

        public MediaItem GetRandomMediaItem()
        {
            MediaItem mi = null;
            List<MediaItem> results = this.MediaItems();

            if (results.Count > 0)
            {
                int iIndex = 0;
                Random r = new Random();
                iIndex = r.Next(results.Count);
                mi = results[iIndex];
            }

            return mi;
        }

    }
}
