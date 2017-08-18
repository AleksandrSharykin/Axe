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
        if (c === '`') {
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

            if (c !== '`') {
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
            html += tag(t, colorize(word), false, style);
            word = '';
            normal = true;
        }

        if (word && word.length > 0) {
            if (normal)
                html += sans(word);
            else {
                var t = block ? 'pre' : 'span';
                html += tag(t, colorize(word), false, style);
            }
        }

        return html;

        function colorize(code) {
            if (!code || !code.length)
                return '';

            var keywords =
                [
                    "alias", "async", "await", "dynamic", "get", "global", "nameof", "orderby", "partial",
                    "var", "when", "yield", "abstract", "as", "base", "bool", "break", "byte", "case",
                    "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate",
                    "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed",
                    "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal",
                    "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
                    "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
                    "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
                    "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
                    "virtual", "void", "volatile", "while"
                ];
            var separators = ' .,!?:;-+*/%^()[]<>{}&|"\'\n\r\t';

            var html = '';
            var word = '';
            var quote = false;

            for (var i = 0; i < code.length; i++) {
                var c = code.charAt(i);

                if (separators.indexOf(c) < 0) {
                    word += c;
                    continue;
                }

                if (c === '"') {
                    word += c;

                    if (quote) {
                        // string closed
                        html += tag('span', word, true, 'class="codeblock-string"');
                        word = '';
                        quote = false;
                    }
                    else quote = true;
                }
                else if (!quote) {
                    if (keywords.indexOf(word) < 0) {
                        html += sans(word)
                    }
                    else {
                        html += tag('span', word, true, 'class="codeblock-keyword"');
                    }
                    word = '';
                    html += sans(c);
                }
                else word += sans(c);
            }
            return html;
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