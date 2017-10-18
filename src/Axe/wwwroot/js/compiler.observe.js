// Declarations
var messageTypes = {
    startedSolveTask: 'StartedSolveTask',
    finishedSolveTask: 'FinishedSolveTask',
    startedObserve: 'StartedObserve',
    finishedObserve: 'FinishedObserve',
    sync: 'Sync',
};

// Web-socket
var socket;

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
            case messageTypes.startedSolveTask:
                console.log('User ID: ' + data.SenderUserId + " started to solve the task: " + data.TaskId);
                var user = $('<td/>').text(data.SenderUserId);
                var task = $('<td/>').text(data.TaskId);
                var technology = $('<td/>');
                var observer = $('<td/>').attr('name', 'observer').text('-');
                var formObserve = $('<form/>').attr('action', '/Compiler/Observe').attr('method', 'post')
                    .append($('<input/>', {
                        name: 'userId',
                        type: 'hidden',
                        value: data.SenderUserId
                    }), $('<input/>', {
                        name: 'taskid',
                        type: 'hidden',
                        value: data.TaskId
                    }), $('<input/>', {
                        type: 'submit',
                        value: 'Observe',
                        class: 'btn btn-primary'
                    }));
                var observeBtn = $('<td/>').append(formObserve);
                var tableLine = $('<tr/>').attr('data-userId', data.SenderUserId)
                    .append([user, task, technology, observer, observeBtn]);
                
                $('#activityTable > tbody:last-child').append(tableLine);

                $.ajax({
                    url: '/api/compiler/GetTaskById',
                    method: 'GET',
                    data: {
                        id: data.TaskId,
                    },
                    success: function (data) {
                        console.log('AJAX: data from server was received');
                        console.log(data);
                        technology.text(data.technology.name);
                    },
                    error: function (e) { console.log(e) }
                });
                $.ajax({
                    url: '/api/users/GetUserById',
                    method: 'POST',
                    data: {
                        id: data.SenderUserId,
                    },
                    success: function (data) {
                        user.text(data.userName);
                    },
                    error: function (e) { console.log(e) }
                });
                break;
            case messageTypes.finishedSolveTask:
                console.log('User ID: ' + data.SenderUserId + " finished to solve the task");
                if ($('#activityTable tr[data-userId=' + "'" + data.SenderUserId + "'" + ']').length > 0)
                {
                    $('#activityTable tr[data-userId=' + "'" + data.SenderUserId + "'" + ']').remove();
                }
                break;
            case messageTypes.startedObserve:
                console.log('User ID: ' + data.SenderUserId + " started to observe the user id: " + data.ObservedUserId);
                var tableLine = $('#activityTable tr[data-userId=' + "'" + data.ObservedUserId + "'" + ']');
                if (tableLine.length > 0) {
                    var btnObserve = $(tableLine).find('input[type="submit"]');
                    if (btnObserve.length > 0) {
                        btnObserve.addClass('disabled');
                        btnObserve.attr('disabled', 'disabled');
                    }
                    $.ajax({
                        url: '/api/users/GetUserById',
                        method: 'POST',
                        data: {
                            id: data.SenderUserId,
                        },
                        success: function (data) {
                            $(tableLine).find('td[name="observer"]').text(data.userName);
                        },
                        error: function (e) { console.log(e) }
                    });
                }
                break;
            case messageTypes.finishedObserve:
                console.log('User ID: ' + data.SenderUserId + " finished to observe the user id: " + data.ObservedUserId);
                var tableLine = $('#activityTable tr[data-userId=' + "'" + data.ObservedUserId + "'" + ']');
                if (tableLine.length > 0) {
                    var btnObserve = $(tableLine).find('input[type="submit"]');
                    btnObserve.removeClass('disabled');
                    btnObserve.removeAttr('disabled');
                    $(tableLine).find('td[name="observer"]').text('-');
                }
                break;
            default:
                console.log('WARNING! Message type is ' + data.Type);
                break;
        }
    };
}

connect();