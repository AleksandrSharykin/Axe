$(document).ready(function () {
    var types = {
        entry: 'Entry',
        question: 'Question',
        answer: 'Answer',
        scores: 'Scores',
        exit: 'Exit',
    };

    var mode;
    if ($('#send_answer').length)
        mode = types.answer;

    if ($('#send_question').length)
        mode = types.question;

    var uid = $('#userId').val();
    var quizId = +$('#quizId').val();

    $('#inbox').find('tr').each(function () {
        var tr = $(this);
        var participant = tr.attr('value');
        var span = tr.find('span');

        span.click(function () {
            span.hide();
            mark({
                userId: uid,
                quizId: quizId,
                text: participant,
                messageType: types.entry
            });
        });

    });

    var scheme = document.location.protocol === 'https:' ? 'wss' : 'ws';
    var port = document.location.port ? (':' + document.location.port) : '';

    // for echo test in Startup
    // var connectionUrl = scheme + '://' + document.location.hostname + port + '/ws';

    var connectionUrl = scheme + '://' + document.location.hostname + port + '/quiz/Participate'

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
        else if (msg.messageType === types.question) {
            // participants received new question
            var p = $('<p class="codeblock md"></p>');
            p.html(p.markdown2html(msg.content));
            $('#question').html(p);

        }
        else if (msg.messageType === types.answer) {
            // judge received new answer from participant                    
            var participant = msg.userId;

            var m = $('<span class="glyphicon glyphicon-check"></span>');
            m.click(function () {
                $(this).hide();
                mark({
                    userId: uid,
                    quizId: quizId,
                    text: participant,
                    messageType: types.entry
                })
            });

            var c = msg.content;
            var td = concatTd([c.userName, c.answer]);

            var tr = $('tr[value=' + msg.userId + ']').first();

            if (!tr.length) {
                tr = $('<tr value="' + msg.userId + '"></tr>');
                tr.html(td);
                $('#inbox').append(tr);
            }
            else {
                tr.html(td)
            }
            tr.append($('<td></td>').append(m));
        }
        else {
            // something else
            console.log('entry: ' + msg.content)
            $('body').append($('<p>' + data + '</p>'))
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
        post = question;
        $('#send_question').click(post);
    }

    // send current input on Ctrl+Enter hotkey
    if (post)
        $('#msg').keydown(function (e) {
            if (e.ctrlKey && (e.keyCode == 13 || e.keyCode == 10)) {
                post();
            }
        });

    function concatTd(arr) {
        return arr.map(function (s) {
            return '<td>' + s + '</td>'
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

    function mark(msg) {
        console.log('marking: ', JSON.stringify(msg));
        $.ajax({
            url: '/quiz/mark',
            method: 'get',
            data: msg,
            success: function (data) { console.log('marked: ', JSON.stringify(data)); },
            error: function (e) { console.log('mark error: ', e); }
        })
    }
})