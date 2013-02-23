using System.Web;
using System.Web.UI;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.StringExtensions;

namespace Outercore.FieldTypes.Common {
  /// <summary>
  /// Injects Scriptaculous scripts into content editor
  /// </summary>
  public class InjectScripts {
    public void Process(PipelineArgs args) {
      if (Sitecore.Context.ClientPage.IsEvent) {
        return;
      }

      HttpContext context = HttpContext.Current;
      if (context == null) {
        return;
      }

      Page page = context.Handler as Page;
      if (page == null) {
        return;
      }

      Assert.IsNotNull(page.Header, "Content Editor <head> tag is missing runat='value'");

      string[] scripts = new[]
        {
          "/sitecore/shell/Controls/Lib/Scriptaculous/Scriptaculous.js", 
          "/sitecore/shell/Controls/Lib/Scriptaculous/builder.js",
          "/sitecore/shell/Controls/Lib/Scriptaculous/effects.js", 
          "/sitecore/shell/Controls/Lib/Scriptaculous/dragdrop.js", 
          "/sitecore/shell/Controls/Lib/Scriptaculous/slider.js", 
          "/sitecore%20modules/outercore.fieldtypes/textlist/js/textlist.js"
        };

      foreach(string script in scripts) {
        page.Header.Controls.Add(new LiteralControl("<script type='text/javascript' language='javascript' src='{0}'></script>".FormatWith(script)));  
      }
    }
  }
}