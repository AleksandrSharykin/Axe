$(document).ready(function () {
    var types = {
        entry: 'Entry',
        question: 'Question',
        answer: 'Answer',
        mark: 'Mark',
        exit: 'Exit'
    };

    var markCorrect = '<label class="label label-success"><span class="glyphicon glyphicon-ok"></span></label>';
    var markWrong = '<label class="label label-danger"><span class="glyphicon glyphicon-remove"></span></label>'

    var mode;
    if ($('#send_answer').length)
        mode = types.answer;

    if ($('#send_question').length)
        mode = types.question;

    var uid = $('#userId').val();
    var quizId = +$('#quizId').val();

    $('#inbox tr').each(function () {
        attachMark($(this));
    });

    var scheme = document.location.protocol === 'https:' ? 'wss' : 'ws';
    var port = document.location.port ? (':' + document.location.port) : '';

    // for echo test in Startup
    // var connectionUrl = scheme + '://' + document.location.hostname + port + '/ws';

    var connectionUrl = scheme + '://' + document.location.hostname + port + '/quiz/Participate';

    console.log(connectionUrl);

    var socket = new WebSocket(connectionUrl);
    socket.onopen = function () { console.log('open'); entry('0'); }
    socket.onclose = function () { console.log('closed'); }
    socket.onerror = function (e) { console.log('error' + e); }
    socket.onmessage = function (response) {
        var data = response.data;

        var msg = JSON.parse(data);
        console.log('received: ' + data)

        if (msg.messageType === types.exit) {
            socket.close();
        }
        else if (msg.messageType === types.entry) {
            var tr = $('#scores tr[value="' + msg.userId + '"]');
            if (tr.length)
                return;
            var content = JSON.parse(msg.content);
            $('#scores').append($('<tr></tr>').attr('value', msg.userId).append([cell(content.userName), cell(content.score)]));
        }
        else if (msg.messageType === types.question) {
            // participants received new question
            var p = $('<p class="codeblock md"></p>');
            p.html(p.markdown2html(msg.content));
            $('#question').html(p);

            $('#mark').text('');
            $('#inbox').text('');
        }
        else if (msg.messageType === types.answer) {
            // judge received new answer from participant
            var content = JSON.parse(msg.content);
            var td = select([content.userName, content.answer], cell);

            var tr = $('#inbox tr[value=' + msg.userId + ']').first();

            if (!tr.length) {
                tr = $('<tr value="' + msg.userId + '"></tr>');
                tr.html(td);
                $('#inbox').append(tr);
            }
            else {
                tr.html(td);
            }

            tr.append(cell(markCorrect)).append(cell(markWrong));
            attachMark(tr);
        }
        else if (msg.messageType === types.mark) {
            // participant received mark for an answer
            $('#mark').html(msg.content === 'True' ? markCorrect : markWrong);
            $('#score').text(msg.text);
        }
        else {
            // something else
            console.log('received: ' + msg.content)
        }
    }

    var post;
    // attach send method to Answer button
    if (mode === types.answer) {
        post = function () {
            var message = answer();
            if (message) {
                $('#inbox').text(message);
            }
        };
        $('#send_answer').click(post);
    }

    // attach send method to Question button
    if (mode === types.question) {
        post = function () {
            var message = question();
            if (message) {
                $('#question').text(message);
            }
        };
        $('#send_question').click(post);
    }

    // send current input on Ctrl+Enter hotkey
    if (post)
        $('#msg').keydown(function (e) {
            if (e.ctrlKey && (e.keyCode === 13 || e.keyCode === 10)) {
                post();
            }
        });

    // creates tbale td element
    function cell(markup) {
        return $('<td></td>').html(markup);
    }

    // applies function to elements of array
    function select(arr, formatter) {
        return arr.map(function (s) {
            return formatter ? formatter(s) : s;
        });
    }

    function attachMark(tr) {
        var participant = tr.attr('value');
        if (!participant)
            return;

        tr.find('.label-success').click(function () {
            mark(participant, true);
            tr.remove();
        });

        tr.find('.label-danger').click(function () {
            mark(participant, false);
            tr.remove();
        });
    }

    // creates message body (json) and sends to quiz controller
    function send(type, text) {
        if (socket.readyState !== 1)
            return;

        var clear;
        if (!text) {
            text = $('#msg').val();
            clear = true;
            $('#msg').focus();
        }

        if (!text)
            return;

        var msg = {
            messageType: type,
            content: text,
            userId: uid,
            quizId: quizId
        };
        socket.send(JSON.stringify(msg));

        if (clear) {
            $('#msg').val(null);
        }

        return text;
    }

    function entry(text) {
        return send(types.entry, text);
    }

    function question(text) {
        console.log('asking')
        return send(types.question, text);
    }

    function answer(text) {
        console.log('answering')
        return send(types.answer, text);
    }

    function mark(participant, success) {
        var msg = {
            userId: uid,
            quizId: quizId,
            messageType: types.mark,
            text: participant,
            content: success
        };

        $.ajax({
            url: '/quiz/mark',
            method: 'get',
            data: msg,
            dataType: 'json',
            success: function (data) {
                var content = JSON.parse(data.content);
                $('#scores tr[value="' + data.text + '"] td').eq(1).text(content.score);
            },
            error: function (e) { console.log('mark error: ', e); }
        })
    }
})