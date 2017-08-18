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

    if (text && text.length > 0) {
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
        //var style = 'style = "text-decoration: underline;"';
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
                block = content.charAt(i - 1) === '\n';
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

        if (word && word.length > 0) {
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
            if (!code || !code.length)
                return '';

            var keywords = syntax.keywords;
            var reserved = syntax.reserved;
            var separators = syntax.separators;
            var q = syntax.quoteChar;
            var casesensitive = syntax.casesensitive;

            var html = '';
            var word = '';
            var quote = false;

            for (var i = 0; i < code.length; i++) {
                var c = code.charAt(i);

                if (separators.indexOf(c) < 0) {
                    word += c;
                    continue;
                }

                if (c === q) {
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
                    separators: ' .,!?:;-+*/%^()[]<>{}&|"\'\n\r\t',
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
                    reserved: ['Main']
                };
                case '#js': return {
                    separators: ' .,!?:;-+*/%^()[]<>{}&|"\'\n\r\t',
                    quoteChar: '\'',
                    casesensitive: true,
                    keywords:
                    [
                        'abstract', 'alias', 'as', 'async', 'await', 'base', 'bool', 'break', 'byte', 'case', 'catch',
                        'char', 'checked', 'class', 'const', 'continue', 'decimal', 'default', 'delegate', 'do',
                        'double', 'dynamic', 'else', 'enum', 'event', 'explicit', 'extern', 'false', 'finally',
                        'fixed', 'float', 'for', 'foreach', 'get', 'global', 'goto', 'if', 'implicit', 'in', 'int',
                        'interface', 'internal', 'is', 'lock', 'long', 'nameof', 'namespace', 'new', 'null', 'object',
                        'operator', 'orderby', 'out', 'override', 'params', 'partial', 'private', 'protected', 'public',
                        'readonly', 'ref', 'return', 'sbyte', 'sealed', 'short', 'sizeof', 'stackalloc', 'static',
                        'string', 'struct', 'switch', 'this', 'throw', 'true', 'try', 'typeof', 'uint', 'ulong',
                        'unchecked', 'unsafe', 'ushort', 'using', 'var', 'virtual', 'void', 'volatile', 'when', 'while', 'yield'
                    ],
                    reserved: ['Array', 'Date', 'eval', 'Function', 'Infinity', 'isFinite', 'isNaN', 'NaN', 'Number', 'parseFloat', 'parseInt', 'String', 'undefined']
                };
                case '#sql': return {
                    separators: ' .,!?:;-+*/%^()[]<>{}&|"\'\n\r\t',
                    quoteChar: '\'',
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
            "'": '&#39;',
            '/': '&#x2F;',
            '`': '&#x60;'
        };
        return String(str).replace(/[&<>"'`=\/]/g, function (s) {
            return entityMap[s];
        });
    }

}