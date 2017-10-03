var editor = CodeMirror.fromTextArea(document.getElementById("sourceCodeTextArea"), {
    lineNumbers: true,
    matchBrackets: true,
    mode: "text/x-csharp"
});

var tagname = $('#selectedTechnology').prop('tagName').toUpperCase();
switch (tagname) {
    case 'INPUT': {
        setupCodeEditor($('#selectedTechnology').val());
        break;
    }
    /*
    case 'SELECT': {
        setupCodeEditor($("#selectedTechnology option:selected").text());
        $('#selectedTechnology').change(function () {
            var selectedTechnology = $("#selectedTechnology option:selected").text();
            setupCodeEditor(selectedTechnology);
        });
        break;
    }
    */
}

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

function setupCodeEditor(technology) {
    console.log('tech: ' + technology + " is active");
    switch (technology) {
        case 'C#': {
            editor.setOption('mode', 'text/x-csharp');
            break;
        }
        case 'JavaScript': {
            editor.setOption('mode', 'text/javascript');
            break;
        }
        case 'Python': {
            editor.setOption('mode', 'text/x-python');
            break;
        }
        default: {
            editor.setOption('mode', 'text/x-csharp');
            break;
        }
    }
}