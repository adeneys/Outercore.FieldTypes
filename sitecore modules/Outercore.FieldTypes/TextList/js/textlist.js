if (typeof (Sitecore) == "undefined") Sitecore = new Object;
if (typeof (Sitecore.FieldTypes) == "undefined") Sitecore.FieldTypes = new Object;

Sitecore.FieldTypes.TextList = Class.create({
  initialize: function(container) {
    this.container = $(container);
    this.list = this.container.down(".textlist-list");
    this.autocomplete = this.container.down(".textlist-autocomplete");
    this.choices = this.container.down(".textlist-choices");

    this.disabled = this.container.getAttribute("disabled");

    this.valueInput = $(this.container.id + "_value");

    if (this.disabled) {
      return;
    }

    this.input = this.insertInput();

    this.container.observe("blur", function() {
      console.warn("blur");
      this.autocomplete.hide();
    } .bind(this));

    this.container.observe("resize", this.adjustInputWidth.bind(this));

    this.list.select(".textlist-closebutton").each(function(button) {
      button.observe("click", this.onCloseButtonClick.bindAsEventListener(this));
    } .bind(this));

    this.setupSortable();

    this.adjustInputWidth();
  },

  addItem: function(text, value) {
    var item = new Element("div", { className: "textlist-item", sc_value: value });
    item.innerHTML = text;

    var closeButton = new Element("a", { href: "#", className: "textlist-closebutton" });
    closeButton.observe("click", this.onCloseButtonClick.bindAsEventListener(this));

    item.insert(closeButton);

    this.list.insert(item);

    this.setupSortable();
    this.update();
  },

  addItemFromInput: function() {
    if (this.addItemFromAutocomplete()) {
      return;
    }

    var text = this.input.value;
    this.input.value = "";

    this.addItem(text, "sc_novalueyet");
  },

  addItemFromAutocomplete: function() {
    if (this.autocomplete.childNodes.length == 0) {
      return false;
    }

    var selected = this.autocomplete.down(".selected span");

    var text = selected.getAttribute("sc_text");
    var value = selected.getAttribute("sc_value");

    this.addItem(text, value);

    this.input.value = "";
    this.autocomplete.hide();

    return true;
  },

  adjustInputWidth: function() {
    var totalWidth = this.container.getWidth();
    var offset = 0;

    if (this.list.childNodes.length > 0) {
      var lastItem = $(this.list.childNodes[this.list.childNodes.length - 1]);
      offset = lastItem.offsetLeft + lastItem.getWidth();
    }

    var width = Math.max(totalWidth - offset - 16, 12) + "px"

    this.input.setStyle({ width: width });
  },

  autocompleteUp: function() {
    console.info("autocomplete up");

    if (this.autocomplete.childNodes.length < 2) {
      return;
    }

    var selected = this.autocomplete.down(".selected");
    selected.removeClassName("selected");

    selected = selected.previous() ? selected.previous() : selected;
    selected.addClassName("selected");

    var lineTop = selected.offsetTop;
    var portTop = this.autocomplete.scrollTop;

    if (lineTop < portTop) {
      console.warn("scrolling to %s", lineTop);
      selected.scrollIntoView();
    }
  },

  autocompleteDown: function() {
    console.info("autocomplete down");

    if (this.autocomplete.childNodes.length < 2) {
      return;
    }

    var selected = this.autocomplete.down(".selected");
    selected.removeClassName("selected");

    selected = selected.next() ? selected.next() : selected;
    selected.addClassName("selected");

    var lineTop = selected.offsetTop;
    var portTop = this.autocomplete.scrollTop;

    var sum = portTop + this.autocomplete.getHeight() - selected.getHeight();

    if (lineTop > sum) {
      console.warn("scrolling to %s", lineTop);
      selected.scrollIntoView();
    }
  },

  getValue: function() {
    return this.list.select(".textlist-item").map(function(item) { return item.getAttribute("sc_value"); }).join("|");
  },

  highlight: function(text, pattern) {
    var regex = new RegExp("(" + pattern + ")", "i");
    return text.replace(regex, "<em>$1</em>");
  },

  insertInput: function() {
    var input = new Element("input", { className: "textlist-input", type: "text" });

    input.observe("keydown", this.onInputKeyDown.bindAsEventListener(this));
    input.observe("keyup", function(e) { this.updateAutocomplete(this.input.value, e); } .bindAsEventListener(this));

    this.container.insert(input);

    return input;
  },

  onCloseButtonClick: function(e) {
    var item = e.target.up(".textlist-item");
    this.removeItem(item);
    e.stop();
  },

  onInputKeyDown: function(e) {
    if (e.keyCode == 27) {
      this.autocomplete.hide();
      e.stop();
      return;
    }

    if (e.keyCode == 9 || e.keyCode == 13) {
      this.addItemFromInput();
      e.stop();
    }

    if (e.keyCode == 38) {
      this.autocompleteUp();
      e.stop();
    }

    if (e.keyCode == 40) {
      this.autocompleteDown();
      e.stop();
    }
  },

  removeItem: function(item) {
    var list = this;

    item.fade({
      duration: 0.2,
      afterFinish: function() {
        item.remove();
        list.update();
      }
    });
  },

  updateAutocomplete: function(text, e) {
    console.info("update autocomplete");

    if (e.keyCode == 38 || e.keyCode == 40 || e.keyCode == 27) {
      return;
    }

    var autocomplete = this.autocomplete;
    var textlist = this;

    var matches;

    if (text.length > 0) {
      matches = this.choices.select("span").findAll(function(span) {
        return span.innerHTML.toLowerCase().indexOf(text) >= 0;
      });
    }
    else {
      matches = this.choices.select("span");
    }

    if (matches.length == 0) {
      autocomplete.hide();
      return;
    }

    autocomplete.innerHTML = "";

    var first = true;

    $A(matches).each(function(match) {
      var item = new Element("div", { className: first ? "item selected" : "item" });
      first = false;

      var span = new Element("span", { sc_text: match.getAttribute("sc_text"), sc_value: match.getAttribute("sc_value") });
      span.innerHTML = textlist.highlight(match.innerHTML, text);

      item.insert(span);

      autocomplete.insert(item);

      item.observe("mouseover", function() {
        var selected = autocomplete.down(".selected");

        if (this != selected) {
          selected.removeClassName("selected");
          this.addClassName("selected");
        }
      });

      item.observe("click", function() { textlist.addItemFromAutocomplete(); });
    });

    var itemsToShow = Math.min(10, matches.length);

    autocomplete.show();

    autocomplete.setStyle({ height: autocomplete.down(".item").getHeight() * itemsToShow });
  },

  setupSortable: function() {
    Sortable.create(this.list.id, {
      tag: "div",
      overlap: "horizontal",
      constraint: "horizontal"
    });

    this.list.observe("mouseup", function() { this.update(); } .bind(this));

    this.list.select(".textlist-item").each(function(item) {
      item.observe("mouseover", function() { this.addClassName("textlist-item-hover"); });
    });

    this.list.select(".textlist-item").each(function(item) {
      item.observe("mouseout", function() { this.removeClassName("textlist-item-hover"); });
    });
  },

  update: function() {
    var value = this.getValue();
    if (value != this.valueInput.value) {
      this.valueInput.value = value;
      scForm.setModified(true);
    }

    this.adjustInputWidth();
  }
});
