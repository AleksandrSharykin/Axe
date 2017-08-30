$(document).ready(function () {
    var types = {
        entry: 'entry',
        question: 'question',
        answer: 'answer',
        scores: 'scores'
    };

    var uid = $('#userId').val();
    var quizId = +$('#quizId').val();

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
        //console.log(data)
        //console.log(typeof data)

        var msg = JSON.parse(data);

        var p = $('<p class="codeblock md"></p>');
        p.html(p.markdown2html(msg.content));

        if (msg.messageType === types.question) {
            $('#question').html(p);
        }
        else {
            $('#inbox').append(p);
        }

    }
    var post;
    if ($('#send_answer').length) {
        post = answer;
        $('#send_answer').click(function () {
            answer();
        });
    }

    if ($('#send_question').length) {
        post = question;
        $('#send_question').click(function () {
            question();
        });
    }

    if (post)
        $('#msg').keydown(function (e) {
            if (e.ctrlKey && (e.keyCode == 13 || e.keyCode == 10)) {
                post();
            }
        });

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
    }

    function entry(text) {
        send(types.entry, text);
    }

    function question(text) {
        console.log('asking')
        send(types.question, text);
    }

    function answer(text) {
        console.log('answering')
        send(types.answer, text);
    }
})