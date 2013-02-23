var scCarousel;

Event.observe(window, "load", function() {
  var carousel = new UI.Carousel("horizontal_carousel");
  scCarousel = new Sitecore.Carousel(carousel);
});

if (typeof(Sitecore) == "undefined") {
  Sitecore = new Object();
}

Sitecore.Carousel = Class.create({
  initialize: function(carousel) {
    this._carousel = carousel;
    var index = 0;
    
    var ul = $$("#horizontal_carousel ul")[0];
    if (ul && ul.hasClassName("disabled")) {
      this.disabled = true;
      $(document.body).setAttribute("disabled", "true");
      return;
    }
  
    $$("ul li").each(function(item) {
      item.observe("click", function(e) {
        scCarousel.click(item);
      }.bindAsEventListener(item));

      item.observe("mouseover", function(e) {
        $(item).addClassName("hover");
      }.bindAsEventListener(item));

      item.observe("mouseout", function(e) {
        $(item).removeClassName("hover");
      }.bindAsEventListener(item));
      
      if (item.hasClassName("selected")) {
        this._carousel.scrollTo(index);
      }
      
      index++;
    }.bind(this));
    
    this.fieldID = $$(".field_id")[0].value;
    this.selected = window.parent.document.getElementById(this.fieldID + "_selected");
  },
  
  click: function(item) {
    var prev_selected = $$("ul li").find(function(item) { return item.hasClassName("selected"); });
    if (prev_selected != null) {
      prev_selected.removeClassName("selected");
      prev_selected.removeClassName("hover");
    }
    
    if (item != prev_selected) {
      $(item).addClassName("selected");
      this.selected.value = item.id;
    }
    else {
      this.selected.value = "";
    }
    
    scForm.getParentForm().setModified(true);
  }
});