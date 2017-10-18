// Timers
var syncTimer;
var autosavingTimer;
var SYNC_TIMEOUT = 15000;
var AUTOSAVING_TIMEOUT = 20000;

// Connection settings
var scheme = document.location.protocol === "https:" ? "wss" : "ws";
var port = document.location.port ? (":" + document.location.port) : "";
var connectionUrl = scheme + "://" + document.location.hostname + port + "/compiler/ConnectAsUser";

function close() {
    if (!socket || socket.readyState !== WebSocket.OPEN) {
        alert("Socket isn't connected");
    }
    socket.close(1000, "Closing from client");
};

function send(message) {
    if (!socket || socket.readyState !== WebSocket.OPEN) {
        alert("Socket isn't connected");
    }
    socket.send(message);
}

function connect() {
    console.log('Try to connect with web-socket...');
    console.log(connectionUrl);
    socket = new WebSocket(connectionUrl);

    socket.onopen = function (event) {
        console.log('Socket was opened');
        var taskId = $('#taskId').val();
        var data = {
            Type: messageTypes.startedSolveTask,
            TaskId: taskId,
        };
        console.log('Send that user started to solve the task: ' + taskId);
        socket.send(JSON.stringify(data));
    };

    socket.onclose = function (event) {
        console.log('Socket was closed');
        if (syncTimer) {
            clearInterval(syncTimer);
        }
    };

    socket.onerror = function (event) {
        console.log('Socket-error');
        console.log(event);
        if (syncTimer) {
            clearInterval(syncTimer);
        }
        alert('An error occurred with web-socket connection. Try to reload your page.');
    };

    socket.onmessage = function (event) {
        var data = JSON.parse(event.data);
        console.log('New message from: ' + data.SenderUserId);
        console.log('Type of message is: ' + data.Type);

        switch (data.Type) {
            case messageTypes.madeChanges:
                var changesObj = JSON.parse(data.Text);
                console.log(changesObj);
                var insertedText = "";
                for (var i = 0; i < changesObj.text.length; i++) {
                    insertedText += changesObj.text[i];
                    if (changesObj.text.length > 1 && i < changesObj.text.length - 1) {
                        insertedText += '\n';
                    }
                }
                editor.getDoc().replaceRange(insertedText, changesObj.from, changesObj.to);
                console.log('Content of code editor was changed');
                break;
            case messageTypes.startedObserve:
                console.log('You are observed by mentor: ' + data.SenderUserId);
                addAlert('You are observed by mentor', 'info');
                var data = {
                    Text: editor.getDoc().getValue(),
                    Type: messageTypes.sync,
                };
                console.log('The First Sync');
                console.log(data);
                socket.send(JSON.stringify(data));
                (function () {
                    var counter = 0;
                    syncTimer = setInterval(function () {
                        console.log('Sync request #' + counter++);
                        var data = {
                            Text: editor.getDoc().getValue(),
                            Type: messageTypes.sync,
                        };
                        socket.send(JSON.stringify(data));
                    }, SYNC_TIMEOUT);
                })();
                break;
            case messageTypes.finishedObserve:
                console.log('Mentor disconnected: ' + data.SenderUserId);
                addAlert('Mentor disconnected', 'info');
                clearInterval(syncTimer);
                break;
            default: {
                console.log('WARNING! Message type is ' + data.Type + ' and it isn\'t handled');
                break;
            }
        }
    };
}

$('#btnCheck').click(function () {
    $('#btnCheck').attr('disabled', true);
    var timer;
    (function () {
        var i = 1;
        var countOfDelimeters = 3;
        timer = setInterval(function () {
            if (i > countOfDelimeters)
                i = 1;
            $('#btnCheck').attr('value', 'Checking' + '.'.repeat(i));
            i++;
        }, 400);
    })();
    $.ajax({
        url: '/api/compiler/Check',
        method: 'POST',
        data: {
            Id: $('#taskId').val(),
            SourceCode: editor.getDoc().getValue(),
            TechnologyName: $('#selectedTechnology').val(),
        },
        success: function (data) {
            var dataObj = JSON.parse(data);
            $('#divOutput').show();
            if (dataObj.TypeResult.toUpperCase() === 'SUCCESS') {
                $('#outputPanel').removeClass().addClass('panel panel-success');
                $('#outputPanel .panel-heading').empty().append($('<b/>').text('Result'));
                $('#outputPanel .panel-body').empty().append($('<p/>').text('All is done right'));
            } else {
                $('#outputPanel').removeClass().addClass('panel panel-danger');
                $('#outputPanel .panel-heading').empty().append('<b>Errors<b>');
                $('#outputPanel .panel-body').empty().append($('<p/>').append(dataObj.Content.join('<br/>')));
            }
            $('#btnCheck').attr('disabled', false).attr('value', 'Check');
            if (timer) clearInterval(timer);
        },
        error: function (e) {
            console.log(e);
            $('#divOutput').hide();
            $('#btnCheck').attr('disabled', false).attr('value', 'Check');
            addAlert('An error occurred. Try to check again.', 'danger');
            if (timer) clearInterval(timer);
        }
    });
});

$('#btnSave').click(function () {
    saveAttempt().then(() => {
        addAlert('Saved successfully', 'success');
    }).catch(error => {
        addAlert(error, 'danger');
    });
});

function saveAttempt() {
    return new Promise((resolve, reject) => {
        console.log('Try to save attempt...');
        $.ajax({
            url: '/api/compiler/SaveAttempt',
            method: 'POST',
            data: {
                codeBlockId: $('#taskId').val(),
                sourceCode: editor.getDoc().getValue(),
            },
            success: function () {
                console.log('Attempt was saved');
                resolve();
            },
            error: function (e) {
                console.log(e);
                reject(e);
            }
        });
    })
}

$('#btnAutosave').click(function () {
    var state = $(this).attr('aria-pressed');
    if (state !== 'true') {
        addAlert('Autosaving enabled', 'info');
        console.log('Autosaving enabled');
        saveAttempt().then();
        autosavingTimer = setInterval(() => {
            saveAttempt().then();
        }, AUTOSAVING_TIMEOUT);
    } else {
        addAlert('Autosaving disabled', 'info');
        console.log('Autosaving disabled');
        if (autosavingTimer) {
            clearInterval(autosavingTimer);
        }
    }
});

// Delete initial alert with date of last changes
setTimeout(() => {
    if ($('#alertLastChanges').length > 0) {
        console.log('delete alertLastChanges');
        $('#alertLastChanges').remove();
    }
}, 6000);

$(window).bind("beforeunload", function () {
    var data = {
        Type: messageTypes.finishedSolveTask,
    };

    var dataString = JSON.stringify(data);
    console.log('Send that user finished to solve task');
    console.log(dataString);
    socket.send(dataString);
})

connect();