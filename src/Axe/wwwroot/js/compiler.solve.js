var messageTypes = {
    startedSolveTask: 'StartedSolveTask',
    finishedSolveTask: 'FinishedSolveTask',
    madeChanges: 'MadeChanges',
    startedObserve: 'StartedObserve',
    finishedObserve: 'FinishedObserve',
    sync: 'Sync'
};
var ALERT_TIMEOUT = 5000;

// CodeMirror editor
var editor = CodeMirror.fromTextArea(document.getElementById("sourceCodeTextArea"), {
    lineNumbers: true,
    matchBrackets: true,
    mode: "text/x-csharp"
});

// Setup mode for CodeMirror
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

// Allows to setup mode for CodeMirror is according to technology
function setupCodeEditor(technology) {
    console.log('Technology: ' + technology + " is active");
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

// Web-socket
var socket;

editor.on("change", function (editor, change) {
    if (!change.origin || change.origin === 'setValue') {
        return;
    }

    console.log('Content of editor was changed');
    console.log(change);

    var stringChanges = JSON.stringify(change);
    var data = {
        Text: stringChanges,
        Type: messageTypes.madeChanges,
    };

    var dataString = JSON.stringify(data);
    console.log('Send that content of editor was changed: ' + data.Text);
    console.log(dataString);
    socket.send(dataString);
});

function addAlert(message, type) {
    var alert = $('<div/>').addClass('alert');

    switch (type.toUpperCase()) {
        case 'WARNING': {
            alert.addClass('alert-warning');
            alert.append('<strong>WARNING!<strong> ');
            break;
        }
        case 'DANGER': {
            alert.addClass('alert-danger');
            alert.append('<strong>ERROR!<strong> ');
            break;
        }
        case 'SUCCESS': {
            alert.addClass('alert-success');
            alert.append('<strong>SUCCESS!<strong> ');
            break;
        }
        default: {
            alert.addClass('alert-info');
            alert.append('<strong>INFO!<strong> ');
            break;
        }
    }
    alert.append(message);

    $('#divMessages').append(alert);
    setTimeout(() => {
        alert.remove();
    }, ALERT_TIMEOUT);
}