$(document).ready(function () {

    $('input[type=datetime]').datepicker();

    $('input[type=time]').timesetter();

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
        $(this).html($(this).markdown2html(content));
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

    // decorate diagram figures    
    $('.figure').figure()//.each(function () { $(this).addClass('bg-danger') });
    //$('.figure').eq(1).figure('toggleContent');

});

// https://learn.jquery.com/plugins/stateful-plugins-with-widget-factory/
// https://learn.jquery.com/jquery-ui/how-jquery-ui-works/
// https://learn.jquery.com/jquery-ui/widget-factory/widget-method-invocation/
$.widget("axe.figure", {

    // default options
    options: {
        value: 10,
        isContentExpanded: true,
        header: null,
        headerToggle: null,
        content: null,
        footer: null,
    },

    // .ctor
    _create: function () {
        var header;
        var headerToggle;
        var content;
        var footer;

        header = this.element.find('.figure-header').first();

        if (header.length) {
            headerToggle = header.children('.figure-header-toggle').first()
            if (!headerToggle.length) {
                headerToggle = $('<span class="glyphicon glyphicon-chevron-up figure-header-toggle"></span>');
                header.append(headerToggle);
            }

            var self = this;
            headerToggle.click(function () { self.toggleContent(); });
        }

        content = this.element.find('.figure-content').first();

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

                itemToggle.click()
            });
        }

        footer = this.element.find('.figure-footer').first();
        if (!footer.length) {
            footer = $('<div class="figure-footer"></div>');
            this.element.children('.figure-main').append(footer);
        }

        this.options.header = header;
        this.options.headerToggle = headerToggle;
        this.options.content = content;
        this.options.footer = footer;
    },

    // public method to toggle figure
    toggleContent: function (flag) {
        var o = this.options;
        o.content.toggle();
        o.isContentExpanded = !o.isContentExpanded;
        if (o.isContentExpanded)
            o.headerToggle.addClass('glyphicon-chevron-up').removeClass('glyphicon-chevron-down');
        else
            o.headerToggle.addClass('glyphicon-chevron-down').removeClass('glyphicon-chevron-up');
    }
});

// https://learn.jquery.com/plugins/basic-plugin-creation/
// https://learn.jquery.com/plugins/advanced-plugin-concepts/
(function ($) {
    $.fn.markdown2html = function (content) {
        return md2html(content);

        function md2html(str) {
            if (!str) {
                return '';
            }

            var html = '';
            var blocks = [];
            var text = '';

            var escaped = false;
            var op = 0;
            var close = 0;
            var decorated = false;

            for (var i = 0; i < str.length; i++) {
                var c = str.charAt(i);

                // detect code-block 
                if (c === '\u0060') {
                    escaped = !escaped;
                    text += c;
                    continue;
                }

                // text inside code-block displayed as is, without md
                if (escaped) {
                    text += c;
                    continue;
                }

                if (c !== '*') {
                    if (op > 0) {
                        decorated = true;

                        // found some * substring, but not enough to finish decoration; append * as is
                        if (close) {
                            text += Array(close + 1).join('*');
                            close = 0;
                        }
                    }
                    text += c;
                }
                else {
                    // remember substring without decorations
                    if (!op && text) {
                        blocks.push({ power: 0, text: text });
                        text = '';
                    }

                    if (decorated) {
                        close++;
                        if (close === op) {
                            // decoration finished, remember decorated substring
                            blocks.push({ power: op, text: text });
                            op = close = 0;
                            decorated = false;
                            text = '';
                        }
                    }
                    else {
                        op++;
                    }
                }
            }

            if (text) {
                // append last piece of text
                blocks.push({ power: op, text: text });
            }
            else if (op > 0) {
                // append trailing *
                blocks.push({ power: op, text: Array(op + 1).join('*') });
            }

            // create normal, italic or bold blocks  
            for (var j = 0; j < blocks.length; j++) {
                var b = blocks[j];
                b.text = highlight(b.text);


                if (b.power === 0) {
                    html += b.text;
                }
                else if (b.power === 1) {
                    html += tag('em', b.text);
                }
                else {
                    html += tag('strong', b.text);
                }
            }

            return html;

            function tag(t, content, clear, attr) {
                return '<' + t + (attr ? ' ' + attr + ' ' : '') + '>' +
                    (clear ? sans(content) : content) +
                    '</' + t + '>';
            }

            function highlight(content) {
                if (!content) return '';

                var html = '';
                var word = '';
                var block = false;
                var normal = true;
                var style = '';

                for (var i = 0; i < content.length; i++) {
                    var c = content.charAt(i);

                    if (c !== '\u0060') {
                        word += c;
                        continue;
                    }

                    // open code-block
                    if (normal) {
                        html += sans(word);
                        word = '';
                        normal = false;
                        block = !i || content.charAt(i - 1) === '\n';
                        continue;
                    }

                    var t = 'span';
                    if (block && (content.charAt(i + 1) === '\n' || content.substring(i + 1, i + 3) === '\r\n')) {
                        t = 'pre';
                    }

                    // close code-block
                    html += tag(t, tryColorize(word), false, style);
                    word = '';
                    normal = true;
                }

                if (word) {
                    if (normal)
                        html += sans(word);
                    else {
                        html += tag(block ? 'pre' : 'span', tryColorize(word), false, style);
                    }
                }

                return html;

                function tryColorize(code) {

                    if (code.charAt(0) === '#') {
                        var idx = code.indexOf(';');
                        if (idx > 0) {
                            var syntaxName = code.substring(0, idx).trim();
                            var syntax = tryGetSyntax(syntaxName);

                            if (syntax) {
                                code = code.substring(idx + 1);
                                return colorize(code, syntax);
                            }
                        }
                    }
                    return sans(code);
                }

                function colorize(code, syntax) {
                    if (!code)
                        return '';

                    var keywords = syntax.keywords;
                    var reserved = syntax.reserved;
                    var separators = syntax.separators;
                    var q = syntax.quoteChar;
                    var casesensitive = syntax.casesensitive;

                    var html = '';
                    var word = '';
                    var quote = false;

                    // length + 1 is a hack to append last word without additional check after finsihing a loop
                    for (var i = 0, len = code.length + 1; i < len; i++) {
                        var c = code.charAt(i);

                        if (separators.indexOf(c) < 0) {
                            word += c;
                            continue;
                        }

                        if (c === q || quote && !c) {
                            word += c;

                            if (quote) {
                                // string closed, append and highlight string
                                html += tag('span', word, true, 'class="codeblock-string"');
                                word = '';
                            }

                            quote = !quote;
                        }
                        else if (!quote) {
                            if (keywords.indexOf(word) >= 0 || !casesensitive && keywords.indexOf(word.toLowerCase()) >= 0) {
                                // append and highlight keyword
                                html += tag('span', word, true, 'class="codeblock-keyword"');
                            }
                            else if (reserved.indexOf(word) >= 0 || !casesensitive && reserved.indexOf(word.toLowerCase()) >= 0) {
                                // append and reserved keyword
                                html += tag('span', word, true, 'class="codeblock-reserved"');
                            }
                            else {
                                // append word
                                html += sans(word)
                            }
                            word = '';
                            html += sans(c);
                        }
                        else word += sans(c);
                    }
                    return html;
                }

                function tryGetSyntax(name) {
                    switch (name) {
                        case '#cs': return {
                            separators: ' .,!?:;-+*/%^~()[]<>{}&|"\n\r\t\u0027\u0060',
                            quoteChar: '"',
                            casesensitive: true,
                            keywords:
                            [
                                'alias', 'async', 'await', 'dynamic', 'get', 'global', 'nameof', 'orderby', 'partial',
                                'var', 'when', 'yield', 'abstract', 'as', 'base', 'bool', 'break', 'byte', 'case',
                                'catch', 'char', 'checked', 'class', 'const', 'continue', 'decimal', 'default', 'delegate',
                                'do', 'double', 'else', 'enum', 'event', 'explicit', 'extern', 'false', 'finally', 'fixed',
                                'float', 'for', 'foreach', 'goto', 'if', 'implicit', 'in', 'int', 'interface', 'internal',
                                'is', 'lock', 'long', 'namespace', 'new', 'null', 'object', 'operator', 'out', 'override',
                                'params', 'private', 'protected', 'public', 'readonly', 'ref', 'return', 'sbyte', 'sealed',
                                'short', 'sizeof', 'stackalloc', 'static', 'string', 'struct', 'switch', 'this', 'throw',
                                'true', 'try', 'typeof', 'uint', 'ulong', 'unchecked', 'unsafe', 'ushort', 'using',
                                'virtual', 'void', 'volatile', 'while'
                            ],
                            reserved: ['Main', 'Math']
                        };
                        case '#js': return {
                            separators: ' .,!?:;-+*/%^~()[]<>{}&|"\n\r\t\u0027\u0060',
                            quoteChar: '\u0027',
                            casesensitive: true,
                            keywords:
                            [
                                'abstract', 'alias', 'as', 'async', 'await', 'base', 'bool', 'break', 'byte', 'case', 'catch',
                                'char', 'checked', 'class', 'const', 'continue', 'decimal', 'default', 'delegate', 'do',
                                'double', 'dynamic', 'else', 'enum', 'event', 'explicit', 'extern', 'false', 'finally',
                                'fixed', 'float', 'for', 'foreach', 'function', 'get', 'global', 'goto', 'if', 'implicit', 'in', 'int',
                                'interface', 'internal', 'is', 'lock', 'long', 'nameof', 'namespace', 'new', 'null', 'object',
                                'operator', 'orderby', 'out', 'override', 'params', 'partial', 'private', 'protected', 'public',
                                'readonly', 'ref', 'return', 'sbyte', 'sealed', 'short', 'sizeof', 'stackalloc', 'static',
                                'string', 'struct', 'switch', 'this', 'throw', 'true', 'try', 'typeof', 'uint', 'ulong',
                                'unchecked', 'unsafe', 'ushort', 'using', 'var', 'virtual', 'void', 'volatile', 'when', 'while', 'yield'
                            ],
                            reserved: ['Array', 'Date', 'eval', 'Function', 'Infinity', 'isFinite', 'isNaN', 'NaN', 'Number', 'parseFloat', 'parseInt', 'String', 'undefined']
                        };
                        case '#sql': return {
                            separators: ' .,!?:;-+*/%^~()[]<>{}&|"\n\r\t\u0027\u0060',
                            quoteChar: '\u0027',
                            casesensitive: false,
                            keywords:
                            [
                                'add', 'all', 'alter', 'and', 'any', 'as', 'asc', 'authorization', 'backup', 'begin', 'between', 'break',
                                'browse', 'bulk', 'by', 'cascade', 'case', 'check', 'checkpoint', 'close', 'clustered', 'coalesce',
                                'collate', 'column', 'commit', 'compute', 'constraint', 'contains', 'containstable', 'continue',
                                'convert', 'create', 'cross', 'current', 'current_date', 'current_time', 'current_timestamp',
                                'current_user', 'cursor', 'database', 'dbcc', 'deallocate', 'declare', 'default', 'delete',
                                'deny', 'desc', 'disk', 'distinct', 'distributed', 'double', 'drop', 'dump', 'else', 'end',
                                'errlvl', 'escape', 'except', 'exec', 'execute', 'exists', 'exit', 'external', 'fetch', 'file',
                                'fillfactor', 'for', 'foreign', 'freetext', 'freetexttable', 'from', 'full', 'function', 'goto',
                                'grant', 'group', 'having', 'holdlock', 'identity', 'identity_insert', 'identitycol', 'if', 'in',
                                'index', 'inner', 'insert', 'intersect', 'into', 'is', 'join', 'key', 'kill', 'left', 'like', 'lineno',
                                'load', 'merge', 'national', 'nocheck', 'nonclustered', 'not', 'null', 'nullif', 'of', 'off', 'offsets',
                                'on', 'open', 'opendatasource', 'openquery', 'openrowset', 'openxml', 'option', 'or', 'order', 'outer',
                                'over', 'percent', 'pivot', 'plan', 'precision', 'primary', 'print', 'proc', 'procedure', 'public',
                                'raiserror', 'read', 'readtext', 'reconfigure', 'references', 'replication', 'restore', 'restrict',
                                'return', 'revert', 'revoke', 'right', 'rollback', 'rowcount', 'rowguidcol', 'rule', 'save', 'schema',
                                'securityaudit', 'select', 'semantickeyphrasetable', 'semanticsimilaritydetailstable', 'semanticsimilaritytable',
                                'session_user', 'set', 'setuser', 'shutdown', 'some', 'statistics', 'system_user', 'table', 'tablesample',
                                'textsize', 'then', 'to', 'top', 'tran', 'transaction', 'trigger', 'truncate', 'try_convert', 'tsequal',
                                'union', 'unique', 'unpivot', 'update', 'updatetext', 'use', 'user', 'values', 'varying', 'view',
                                'waitfor', 'when', 'where', 'while', 'with', 'within group', 'writetext'
                            ],
                            reserved:
                            [
                                'absolute', 'action', 'ada', 'add', 'all', 'allocate', 'alter', 'and', 'any', 'are', 'as', 'asc',
                                'assertion', 'at', 'authorization', 'avg', 'begin', 'between', 'bit', 'bit_length', 'both', 'by',
                                'cascade', 'cascaded', 'case', 'cast', 'catalog', 'char', 'char_length', 'character', 'character_length',
                                'check', 'close', 'coalesce', 'collate', 'collation', 'column', 'commit', 'connect', 'connection',
                                'constraint', 'constraints', 'continue', 'convert', 'corresponding', 'count', 'create',
                                'cross', 'current', 'current_date', 'current_time', 'current_timestamp', 'current_user',
                                'cursor', 'date', 'day', 'deallocate', 'dec', 'decimal', 'declare', 'default', 'deferrable',
                                'deferred', 'delete', 'desc', 'describe', 'descriptor', 'diagnostics', 'disconnect', 'distinct',
                                'domain', 'double', 'drop', 'else', 'end', 'end-exec', 'escape', 'except', 'exception', 'exec',
                                'execute', 'exists', 'external', 'extract', 'false', 'fetch', 'first', 'float', 'for', 'foreign',
                                'fortran', 'found', 'from', 'full', 'get', 'global', 'go', 'goto', 'grant', 'group', 'having', 'hour',
                                'identity', 'immediate', 'in', 'include', 'index', 'indicator', 'initially', 'inner', 'input', 'insensitive',
                                'insert', 'int', 'integer', 'intersect', 'interval', 'into', 'is', 'isolation', 'join', 'key', 'language',
                                'last', 'leading', 'left', 'level', 'like', 'local', 'lower', 'match', 'max', 'min', 'minute', 'module',
                                'month', 'names', 'national', 'natural', 'nchar', 'next', 'no', 'none', 'not', 'null', 'nullif', 'numeric',
                                'octet_length', 'of', 'on', 'only', 'open', 'option', 'or', 'order', 'outer', 'output', 'overlaps', 'pad',
                                'partial', 'pascal', 'position', 'precision', 'prepare', 'preserve', 'primary', 'prior', 'privileges',
                                'procedure', 'public', 'read', 'real', 'references', 'relative', 'restrict', 'revoke', 'right', 'rollback',
                                'rows', 'schema', 'scroll', 'second', 'section', 'select', 'session', 'session_user', 'set', 'size', 'smallint',
                                'some', 'space', 'sql', 'sqlca', 'sqlcode', 'sqlerror', 'sqlstate', 'sqlwarning', 'substring', 'sum', 'system_user',
                                'table', 'temporary', 'then', 'time', 'timestamp', 'timezone_hour', 'timezone_minute', 'to', 'trailing', 'transaction',
                                'translate', 'translation', 'trim', 'true', 'union', 'unique', 'unknown', 'update', 'upper', 'usage', 'user', 'using',
                                'value', 'values', 'varchar', 'varying', 'view', 'when', 'whenever', 'where', 'with', 'work', 'write', 'year', 'zone'
                            ]
                        };
                    }

                    return null;
                }
            }

            function sans(str) {
                // function to escape HTML tags in a string
                // https://stackoverflow.com/a/12034334/1506454

                var entityMap = {
                    '&': '&amp;',
                    '=': '&#x3D;',
                    '<': '&lt;',
                    '>': '&gt;',
                    '"': '&quot;',
                    '\u0027': '&#39;',
                    '/': '&#x2F;',
                    '\u0060': '&#x60;'
                };
                return String(str).replace(/[&<>"'`=\/]/g, function (s) {
                    return entityMap[s];
                });
            }

        }
    };
}(jQuery));


(function ($) {
    $.fn.timesetter = function () {
        return this.each(function () {
            var self = $(this);
            self.change(displayCurrentTime)

            var nl = '\n';

            // inserting time selector into html
            var clockHtml =
                '<table><tbody>' + nl +

                '<tr>' + nl +
                '<td class="timesetter-hours-decrement" > <span class="glyphicon glyphicon-chevron-up"></span></td >' + nl +
                '<td class="timesetter-minutes-decrement"><span class="glyphicon glyphicon-chevron-up"></span></td>' + nl +
                '</tr >' +
                '<tr>' +
                '<tr><td></td><td></td></tr>' + nl +
                '<tr class="active bg-primary"><td></td><td></td></tr>' + nl +
                '<tr><td></td><td></td></tr>' + nl +

                '<tr>' + nl +
                '<td class="timesetter-hours-increment" > <span class="glyphicon glyphicon-chevron-down"></span></td >' + nl +
                '<td class="timesetter-minutes-increment"><span class="glyphicon glyphicon-chevron-down"></span></td>' + nl +
                '</tr >' +

                '</tbody></table>';

            var clockDiv = $('<div class="timesetter"></div>').html(clockHtml);
            self.parent().append(clockDiv);

            displayCurrentTime();

            // function to update date input after up/down clicked
            clockDiv.find('[class^=timesetter-]').click(function () {
                var timeClass = Array.prototype.filter.call(this.classList, function (c) { return c.indexOf('timesetter') >= 0; })[0];
                if (!timeClass)
                    return;

                var parts = timeClass.split('-');
                var datepart = parts[1];
                var op = parts[2];

                var currentTime = timesetter(clockDiv, datepart, op, getElementIndex(this))
                self.val(currentTime);
            });

            function displayCurrentTime() {
                // determine initial time value
                var hours, minutes;

                var date = self.val();
                if (date) {
                    date = date.split(':');
                    hours = date[0];
                    minutes = date[1];
                }
                else {
                    var d = new Date();
                    hours = getCircularValue(d.getHours(), 23);
                    minutes = getCircularValue(d.getMinutes(), 59);
                }

                timesetter(clockDiv, "hours", "set", 0, +hours);
                timesetter(clockDiv, "minutes", "set", 1, +minutes);
            }

            function timesetter(clock, timepart, op, idx, initialValue) {
                var delta = op === 'increment' ? 1 : (op === 'decrement' ? -1 : 0);
                //if (!delta)
                //    return;

                var central = $(clock).find('tr.active');

                var tdCentral = central.children('td').eq(idx);
                var value = +initialValue || +tdCentral.text();

                var max = (timepart === 'hours') ? 23 : 59;

                value = getCircularValue(value + delta, max);

                tdCentral.text(value);

                central.prev().children('td').eq(idx).text(getCircularValue(+value - 1, max));
                central.next().children('td').eq(idx).text(getCircularValue(+value + 1, max));

                return Array.prototype.map.call(central.children('td'), function (td) { return $(td).text(); }).join(':');
            }

            function getCircularValue(int, max) {
                if (int > max)
                    int = 0;
                if (int < 0)
                    int = max;
                if (int < 10)
                    int = '0' + int;
                return int;
            }

            // https://stackoverflow.com/questions/11761881/javascript-dom-find-element-index-in-container
            // https://stackoverflow.com/a/11762035/1506454
            function getElementIndex(node) {
                var index = 0;
                while ((node = node.previousElementSibling)) {
                    index++;
                }
                return index;
            }
        });
    };
}(jQuery))