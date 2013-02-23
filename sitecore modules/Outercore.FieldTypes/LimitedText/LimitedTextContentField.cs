using System;
using System.Web.UI;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.StringExtensions;
using Sitecore.Text;

namespace Outercore.FieldTypes {
  /// <summary>
  /// Defines the limited text content field class.
  /// </summary>
  public class LimitedTextContentField : Text {
    #region Properties

    public string ItemID { get; set; }
    public string Source { get; set; }
    protected int CharacterLimit { get; private set; }
    protected bool PreventTyping { get; private set; }

    #endregion

    #region Protected methods

    /// <summary>
    /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"></see> object, which writes the content to be rendered on the client.
    /// </summary>
    /// <param name="output">The <see cref="T:System.Web.UI.HtmlTextWriter"></see> object that receives the server control content.</param>
    protected override void DoRender(HtmlTextWriter output) {
      ParseParameters(Source);

      base.DoRender(output);

      output.Write("<span style='position: absolute; top: -14px; right: 0px; color: #333333' ID='{0}_display'>{1}</span>".FormatWith(ID, CharacterLimit - Value.Length));
      output.Write("<input type='hidden' ID='{0}_limit' value='{1}' />".FormatWith(ID, CharacterLimit));
      output.Write("<input type='hidden' ID='{0}_previousRemaining' value='{1}' />".FormatWith(ID, CharacterLimit));

      RenderScript(output);
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Renders the supporting javascript
    /// </summary>
    /// <param name="output">The writer.</param>
    void RenderScript(HtmlTextWriter output) {
      string script = @"
      function scLimitedFieldUpdateCount(id) {
        var input = $(id);
        var limitNode = $('{ID}_limit');
        var displayNode = $('{ID}_display');
        var previousRemainingNode = $('{ID}_previousRemaining');

        var limit = parseInt(limitNode.value);
        var previousRemaining = parseInt(previousRemainingNode.value);
        
        var remaining = limit - input.value.length;
        displayNode.innerHTML = remaining;
        if (remaining >= 0) {
          displayNode.setStyle({ color: '#333333', fontWeight: 'normal' });
        }
        else {
          displayNode.setStyle({ color: '#ce130a', fontWeight: 'bold' });
        }

        if (scContent.startValidators && (previousRemaining < 0 && remaining >= 0) || (previousRemaining >= 0 && remaining < 0)) {
          scContent.startValidators();
        }

        previousRemainingNode.value = remaining;
      }

      $('{ID}').up('div').setStyle({ position: 'relative' });

      scLimitedFieldUpdateCount('{ID}');
      $('{ID}').observe('keyup', function() { scLimitedFieldUpdateCount('{ID}'); });      
      ";

      if (ShouldPreventTyping()) {
        string preventScript = Environment.NewLine + @"
          function scLimitedFieldPreventTyping(evt) {
            var input = evt.target;

            var limitNode = $('{ID}_limit');
            var limit = parseInt(limitNode.value);
            var remaining = limit - input.value.length;
            var displayNode = $('{ID}_display');

            if (evt.keyCode != 8 && evt.keyCode != 46 && remaining <= 0) {
              evt.stop();
              if (Scriptaculous) {
                scLimitedFieldPulsate(displayNode);
                setTimeout(function() {
                  scLimitedFieldPulsate(displayNode);
                }, 250);
              }
              return false;
            }
          }

          $('{ID}').observe('keydown', scLimitedFieldPreventTyping);

          function scLimitedFieldPulsate(element) {
            element.setStyle({ color: '#333333', fontWeight: 'normal' });

            new Effect.Morph(element, {
              style: { color: '#CE130a', fontWeight: 'bold' },
              duration: 0.1
            });
          }
        ";

        script += preventScript;
      }

      script = script.Replace("{ID}", ID);
      
      script = "<script type='text/javascript' language='javascript'>" + script + "</script>";

      output.Write(script);
    }

    /// <summary>
    /// Parses the parameters supplied as field source.
    /// </summary>
    /// <remarks>
    /// Both "40" and "Limit=40" are supported for specifing maximum number of characters the field should accept
    /// </remarks>
    /// <param name="source">The source.</param>
    void ParseParameters(string source) {
      if (string.IsNullOrEmpty(source)) {
        return;
      }

      if (!source.Contains("=")) {
        CharacterLimit = int.Parse(source);
        return;
      }

      var parameters = new UrlString(source);
      if (!string.IsNullOrEmpty(parameters.Parameters["Limit"])) {
        CharacterLimit = int.Parse(parameters.Parameters["Limit"]);
      }

      if (!string.IsNullOrEmpty(parameters.Parameters["PreventTyping"])) {
        PreventTyping = MainUtil.GetBool(parameters.Parameters["PreventTyping"], false);
      }
    }

    /// <summary>
    /// Checks context and configuration to see if user should be able to input more characters than specified by the limit
    /// </summary>
    /// <returns>The prevent typing.</returns>
    bool ShouldPreventTyping() {
      Item item = Sitecore.Context.ContentDatabase.GetItem(ItemID);
      if (StandardValuesManager.IsStandardValuesHolder(item)) {
        return false;
      }

      return PreventTyping;
    }

    #endregion
  }
}