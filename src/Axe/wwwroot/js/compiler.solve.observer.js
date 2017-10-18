// Connection settings
var scheme = document.location.protocol === "https:" ? "wss" : "ws";
var port = document.location.port ? (":" + document.location.port) : "";
var connectionUrl = scheme + "://" + document.location.hostname + port + "/compiler/ConnectAsObserver";

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
    console.log('Try to connect with socket...');
    console.log(connectionUrl);
    socket = new WebSocket(connectionUrl);

    socket.onopen = function (event) {
        console.log('Socket was opened');
        editor.setOption('readOnly', true);
        if ($('#observedUserId').length) {
            var observedUserId = $('#observedUserId').val();
            console.log('Observed user id: ' + observedUserId);
            var data = {
                Type: messageTypes.startedObserve,
                ObservedUserId: observedUserId,
            };
            console.log('Send that mentor starts to observe user with id: ' + observedUserId);
            socket.send(JSON.stringify(data));
        }
        else {
            throw new Error("observedUserId element doesn't exist");
        }
    };

    socket.onclose = function (event) {
        console.log('Socket was closed');
    };

    socket.onerror = function (event) {
        console.log('Socket-error');
        console.log(event);
        alert('An error occurred with web-socket connection. Try to reload your page.');
    };

    socket.onmessage = function (event) {
        var data = JSON.parse(event.data);
        console.log('New message from: ' + data.SenderUserId);
        console.log('Type of message is: ' + data.Type);

        switch (data.Type) {
            case messageTypes.madeChanges: {
                var changesObj = JSON.parse(data.Text); // Parse object which contains changes
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
            }
            case messageTypes.finishedSolveTask: {
                console.log('User id: ' + data.SenderUserId + " finished to solve task: " + data.TaskId);
                editor.setOption('readOnly', true);
                addAlert('User finished solve the task', 'warning');
                break;
            }
            case messageTypes.sync: {
                console.log('Sync');
                console.log(data);
                if (editor.getDoc().getValue() !== data.Text) {
                    console.warn('Need update the content')
                    editor.getDoc().setValue(data.Text);
                    addAlert('Data is synchronized', 'info');
                }
                else {
                    console.log('Content is actual');
                }
                editor.setOption('readOnly', false);
                break;
            }
            default: {
                console.log('WARNING! Message type is ' + data.Type + ' and it isn\'t handled');
                break;
            }
        }
    };
}

$(window).bind("beforeunload", function () {
    var data = {
        Type: messageTypes.finishedObserve,
    };

    var dataString = JSON.stringify(data);
    console.log('Send that mentor finished to observe');
    console.log(dataString);
    socket.send(dataString);
})

connect();