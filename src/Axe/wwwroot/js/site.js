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
});
