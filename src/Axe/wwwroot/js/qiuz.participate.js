$(document).ready(function () {

    var scheme = document.location.protocol === 'https:' ? 'wss' : 'ws';
    var port = document.location.port ? (':' + document.location.port) : '';

    // for echo test in Startup
    // var connectionUrl = scheme + '://' + document.location.hostname + port + '/ws';

    var connectionUrl = scheme + '://' + document.location.hostname + port + '/quiz/Participate'

    console.log(connectionUrl);

    var socket = new WebSocket(connectionUrl);
    socket.onopen = function () { console.log('open'); }
    socket.onclose = function () { console.log('closed'); }
    socket.onerror = function (e) { console.log('error' + e); }
    socket.onmessage = function (response) {
        var data = response.data;
        console.log(data)
        console.log(typeof data)

        //var o = JSON.parse(data);
        //for (var prop in o)
        //    console.log(prop + ' = ' + o[prop]);

        var p = $('<p class="codeblock md"></p>');
        //p.text(response.data);
        p.html(p.markdown2html(response.data));
        $('#question').append(p);
    }

    $('#send').click(function () {
        if (socket.readyState !== 1)
            return;
        var text = $('#msg').val();
        if (!text)
            return;
        socket.send(text);
        $('#msg').val(null)
    });
})