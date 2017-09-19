$("form#formSolve").submit(function (e) {
    $('#btnCheck').attr('disabled', true);
    (function () {
        var i = 1;
        var countOfDelimeters = 3;
        setInterval(function () {
            if (i > countOfDelimeters)
                i = 1;
            $('#btnCheck').attr('value', 'Checking' + '.'.repeat(i));
            i++;
        }, 500);
    })();
    return true;
});

var editor = CodeMirror.fromTextArea(document.getElementById("sourceCodeTextArea"), {
    lineNumbers: true,
    matchBrackets: true,
    mode: "text/x-csharp"
});