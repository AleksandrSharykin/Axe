function md2html(str) {
    if (!str) {
        return "";
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
        if (b.power === 0) {
            html += sans(b.text);
        }
        else if (b.power === 1) {
            html += tag('em', b.text);
        }
        else {
            html += tag('strong', b.text);
        }
    }

    return html;

    function tag(t, content) {
        return '<' + t + '>' + sans(content) + '</' + t + '>';
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