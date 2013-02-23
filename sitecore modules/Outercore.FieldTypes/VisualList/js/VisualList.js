if (typeof (Sitecore) == "undefined") {
    Sitecore = new Object();
}

Sitecore.Visuallist = Class.create({
    initialize: function (root) {
        this.root = $(root);
        if (this.root == null) { alert("visuallist root not found"); return; }

        if (this.root.hasClassName("disabled")) {
            $(document.body).setAttribute("disabled", "true");
            return;
        }

        this.fieldID = $$(".control_id")[0].value;
        this.selected = window.parent.document.getElementById(this.fieldID + "_selected");

        $(document.body).observe("mouseup", function () { this.update(); } .bind(this));

        $$("ul li").each(function (item) {
            this.attachEvents(item);
        } .bind(this));

        this.initializeSortable();

        this.serialize();

        var value = this.serialize();
        this.selected.value = value;
    },

    attachEvents: function (item) {
        item.observe("mouseover", function (e) {
            item.addClassName("hover");
        });

        item.observe("mouseout", function (e) {
            item.removeClassName("hover");
        });

        item.down(".delete").observe("click", function (e) {
            e.stop();
            scVisualList.deleteItem(item);
        });
    },

    initializeSortable: function () {
        if (this.sortableInitialized) {
            Sortable.destroy(this.root);
        }

        Sortable.create(this.root, {
            tag: 'li',
            overlap: 'horizontal',
            constraint: false
        });

        this.sortableInitialized = true;
    },

    addItem: function (id) {
        scForm.postRequest("", "", "", "AddItem(\"" + id + "\")");
    },

    clear: function () {
        this.root.innerHTML = "";
        this.update();
    },

    deleteItem: function (item) {
        item.fade({
            duration: 0.4,
            afterFinish: function () {
                item.remove();
                scVisualList.update();
            }
        });
    },

    appendItem: function (html, id) {
        var li = $(document.createElement("li"));
        li.id = id;
        li.innerHTML = html;

        li.setOpacity(0);
        this.attachEvents(li);

        this.root.insert(li);
        this.initializeSortable();

        li.appear({
            duration: 0.4
        });

        this.update();
    },

    update: function () {
        var value = this.serialize();
        if (value != this.selected.value) {
            this.selected.value = value;
            scForm.getParentForm().setModified(true);
            scForm.getParentForm().Content.validate();
        }
    },

    serialize: function () {
        return this.root.select("li").pluck("id").join("|");
    }
})

var scVisualList = null;
Event.observe(window, "load", function () {
    scVisualList = new Sitecore.Visuallist("list");
})