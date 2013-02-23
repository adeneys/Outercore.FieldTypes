#region using

using System;
using System.Web.UI;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web;
using Sitecore.Text;
using Sitecore.StringExtensions;
using System.Text;

#endregion

namespace Outercore.FieldTypes {
  /// <summary>
  /// Defines the slider content field class.
  /// </summary>
  public class SliderContentField : Border, IContentField {
    #region Properties 

    public new bool Disabled {
      get { return MainUtil.GetBool(StringUtil.GetString(ViewState["Disabled"]), false); }
      set { ViewState["Disabled"] = value; }
    }

    public string ItemID { get; set; }
    public string Source { get; set; }
    public new string Value { get; set; }

    string Min { get; set; }
    string Max { get; set; }
    string Interval { get; set; }
    string Values { get; set; }

    #endregion

    #region Rendering methods

    /// <summary>
    /// Renders the control.
    /// </summary>
    /// <param name="output">The output.</param>
    protected override void DoRender(HtmlTextWriter output) {
      ParseParameters(Source);

      output.Write("<div id='{0}' style='padding: 16px 4px 8px 4px' class='scContentControl scSlider'>".FormatWith(ID));
      RenderSlider(output);
      output.Write("</div>");

      RenderScript(output);

      output.Write("<input type='hidden' id='{0}' value='{1}' />".FormatWith(ID + "_selected", Value));
    }

    /// <summary>
    /// Parses the parameters supplied as Source attribute
    /// </summary>
    /// <param name="rawParameters">The raw parameters.</param>
    void ParseParameters(string rawParameters) {
      var parameters = new UrlString(rawParameters);

      Min = parameters["Min"];
      Max = parameters["Max"];
      Interval = parameters["Interval"];

      if (string.IsNullOrEmpty(Min)) {
        Assert.IsTrue(string.IsNullOrEmpty(Max), "Must also supply 'Max' parameter");
      }

      if (string.IsNullOrEmpty(Max)) {
        Assert.IsTrue(string.IsNullOrEmpty(Min), "Must also supply 'Min' parameter");
      }

      if (!string.IsNullOrEmpty(Interval)) {
        Assert.IsTrue(!string.IsNullOrEmpty(Min) && !string.IsNullOrEmpty(Max), "Must supply 'Min' and 'Max' parameters when using 'Interval' parameter");
      }
      
      Values = parameters["Values"];
    }

    /// <summary>
    /// Renders the slider html
    /// </summary>
    /// <param name="output">The output.</param>
    void RenderSlider(HtmlTextWriter output) {
      var slider =
      @"<div id='{0}_track' [Disabled] style='width:100%; position:relative; background-color:#ccc; height:9px; cursor: pointer; background:transparent url({1}/img/track-repeat.png) repeat-x top right'>
			    <div id='{0}_track-left' style='position:absolute; width: 5px; height: 9px; background: transparent url({1}/img/track-left.png) no-repeat top left;'></div>
          <div id='{0}_handle' style='width:10px; height:15px; [Cursor] position: relative'>
            <img src='{1}/img/handle[ImageSuffix].png' alt='' style='float:left'/>
            <div id='{0}_value' style='padding-top: 4px; width: 40px; position: relative; left: -8px; text-align: center; white-space: nowrap'>{2}</div>
          </div>
          <div id='{0}_track-right' style='position:absolute; top: 0px; right: 0px; width: 5px; height: 9px; background: transparent url({1}/img/track-right.png) no-repeat top right;'></div>
		    </div>
		  ".FormatWith(ID, "/sitecore modules/Outercore.FieldTypes/Slider", Value);

      slider = slider.Replace("[Cursor]", Disabled ? string.Empty : "cursor: move;").Replace("[ImageSuffix]", Disabled ? "_d" : string.Empty);
      slider = slider.Replace("[Disabled]", Disabled ? "disabled = 'true'" : string.Empty);

      output.Write(slider);
    }

    /// <summary>
    /// Renders the supporting javascript
    /// </summary>
    /// <param name="output">The output.</param>
    void RenderScript(HtmlTextWriter output) {
      var range = string.Empty; //range: $R(-200, 200),
      if (!string.IsNullOrEmpty(Min) && !string.IsNullOrEmpty(Max)) {
        range = "range: $R({0}, {1}),".FormatWith(Min, Max);
      }

      var values = string.Empty;
      if (!string.IsNullOrEmpty(Values)) {
        values = "values: [{0}],".FormatWith(Values); // values: [-200, -150, -100, -50, 0, 50, 100, 150, 200],
      }
      else if (!string.IsNullOrEmpty(Interval)) {
        var builder = new StringBuilder();
        var minValue = 0.0;
        var maxValue = 0.0;
        var intervalValue = 0.0;

        double.TryParse(Min, out minValue);
        double.TryParse(Max, out maxValue);
        double.TryParse(Interval, out intervalValue);

        for (var i = minValue; i <= maxValue; i += intervalValue) {
          builder.Append(i);

          if (i < maxValue)
            builder.Append(", ");
        }

        values = "values: [{0}],".FormatWith(builder.ToString());
      }

      var value = string.Empty;
      if (!string.IsNullOrEmpty(Value)) {
        value = "sliderValue: {0},".FormatWith(Value);
      }

      var script =
      @"Event.observe(document, 'sc:contenteditorupdated', function() {
          if (!$('{ID}_handle')) return;
          if (typeof(scSlider{ID}) != 'undefined') return;

          new Control.Slider('{ID}_handle', '{ID}_track', {
            {Range}
            {Values}
            {Value}
            {Disabled}
				    onSlide: function(v) { $('{ID}_value').innerHTML = v; },
				    onChange: function(v) { $('{ID}_value').innerHTML = v; $('{ID}_selected').value = v; scForm.setModified(true) }
			    });

          scSlider{ID} = true;
        });
      ".Replace("{ID}", ID).Replace("{Value}", value).Replace("{Range}", range).Replace("{Values}", values).Replace("{Disabled}", Disabled ? "disabled : true," : string.Empty);

      script = "<script type='text/javascript' language='javascript'>" + script + "</script>";

      output.Write(script);
    }

    #endregion

    #region IContentField Members

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <returns>The value of the field.</returns>
    public string GetValue() {
      return WebUtil.GetFormValue(ID + "_selected");
    }

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="value">The value of the field.</param>
    public void SetValue(string value) {
      Value = value;
    }

    #endregion
  }
}