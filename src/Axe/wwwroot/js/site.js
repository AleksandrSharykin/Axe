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

    // https://stackoverflow.com/questions/14267781/sorting-html-table-with-javascript    
    var tables = document.getElementsByTagName('table');
    for (var i = 0; i < tables.length; i++)
        enableTableSort(tables[i]);

    function enableTableSort(dataTable) {
        var thead = dataTable.tHead;
        if (!(thead && thead.rows[0] && thead.rows[0].cells)) return;

        var headers = thead.rows[0].cells;

        for (var c = 0; c < headers.length; c++) {
            (function (idx) {
                var dir = -1;
                headers[c].addEventListener("click", function () {
                    sortByColumn(dataTable, idx, dir = -dir);
                });
            }(c));
        }
    }

    function sortByColumn(dataTable, columnIdx, dir) {
        var tbody = dataTable.tBodies[0];
        var rows = Array.prototype.slice.call(tbody.rows, 0)

        rows = rows.sort(function (a, b) {
            return dir * compareCells(a, b, columnIdx);
        });

        for (var i = 0; i < rows.length; ++i) {
            tbody.appendChild(rows[i]);
        }

        function item(row, column) {
            return row.cells[column].textContent.toLowerCase().trim();
        }

        function compareCells(a, b, column) {
            return item(a, column).localeCompare(item(b, column))
        }
    }

    $('.figure').figure();
});

(function ($) {
    $.fn.figure = function (action) {

        return this.each(function () {
            var self = $(this);
            var header;
            var headerToggle;
            var content;
            var footer;

            var isContentExpanded = true;

            if (action === null || action === undefined || action === "init") {
                init();
            }

            function init() {
                header = self.find('.figure-header').first();

                if (header.length) {
                    headerToggle = header.children('.figure-header-toggle').first()
                    if (!headerToggle.length) {
                        headerToggle = $('<span class="glyphicon glyphicon-chevron-up figure-header-toggle"></span>');
                        header.append(headerToggle);
                    }

                    headerToggle.click(toggleContent);
                }

                content = self.find('.figure-content').first();

                if (content.length) {
                    content.children('dt').each(function () {
                        var dt = $(this);

                        var isItemExpanded = true;
                        var itemToggle = $(this).children('.figure-group-toggle');
                        if (!itemToggle.length) {
                            itemToggle = $('<span class="glyphicon glyphicon-triangle-top figure-group-toggle"></span> ');
                            dt.prepend(itemToggle);
                        }

                        itemToggle.click(function () {
                            dt.nextUntil('dt').toggle();
                            isItemExpanded = !isItemExpanded;
                            if (isItemExpanded)
                                itemToggle.addClass('glyphicon-triangle-top').removeClass('glyphicon-triangle-bottom');
                            else
                                itemToggle.addClass('glyphicon-triangle-bottom').removeClass('glyphicon-triangle-top');
                        });

                        dt.nextUntil('dt', 'dd').each(function () {
                            var pin = $(this).children('.figure-item-pin').first();
                            if (!pin.length)
                                $(this).prepend($('<span class="glyphicon glyphicon-paperclip figure-item-pin">&nbsp;</span>'));
                        });
                    });
                }

                footer = self.find('.figure-footer').first();
                if (!footer.length) {
                    footer = $('<div class="figure-footer"></div>');
                    self.append(footer);
                }
            }

            function toggleContent() {
                content.toggle();
                isContentExpanded = !isContentExpanded;
                if (isContentExpanded)
                    headerToggle.addClass('glyphicon-chevron-up').removeClass('glyphicon-chevron-down');
                else
                    headerToggle.addClass('glyphicon-chevron-down').removeClass('glyphicon-chevron-up');
            }
        });
    };
}(jQuery));
