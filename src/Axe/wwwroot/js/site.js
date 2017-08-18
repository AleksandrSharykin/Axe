$(document).ready(function () {
    // https://stackoverflow.com/questions/18999501/bootstrap-3-keep-selected-tab-on-page-refresh
    // find tabs groups on page
    var tabs = $('ul.nav-tabs');

    // restore selected tab in tabs group 
    tabs.each(function () {

        var tabsId = $(this).attr('id');

        if (tabsId) {
            var selection = localStorage.getItem(tabsId);

            if (selection)
                $("a[href='" + selection + "']").tab("show");
        }
    });

    $(document.body).on("click", "a[data-toggle]", function (event) {
        // when switching tabs, store new selected tab href

        var tabsId = $(this).parents('ul').first().attr('id');

        localStorage.setItem(tabsId, this.getAttribute("href"));
    });


    // decorate markdown
    $('p.md').each(function () {
        var content = $(this).text();
        $(this).html(md2html(content));
    });
});
