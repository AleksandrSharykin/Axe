$(document).ready(function () {
    $('.btn[data-toggle=modal]').click(function () {
        var tr = $(this).parents('tr').first();
        var id = tr.attr('value');

        if (!id)
            return;

        console.log('click id: ', id);
        $.ajax({
            url: '/quiz/details/' + id,
            method: 'get',
            error: function (e) { console.log(e) },
            success: function (data) {
                console.log('ajax: ' + id + ' ' + JSON.stringify(data))

                $('#quizTitle').text(data.title);
                $('#quizJudge').text(data.judge);


                if (data.canEdit) {
                    $('#quizActions').html('<a class="btn btn-primary" href="/quiz/input/' + id + '">Edit</a>');
                }
                else {
                    $('#quizActions').html('');
                }

                var table = $('#quizParticipants');
                table.html('');
                for (var key in data.scores) {
                    var tr = $('<tr></tr>');
                    tr.append($('<td></td>').text(key));
                    tr.append($('<td></td>').text(data.scores[key]));
                    table.append(tr);
                }
            }
        })
    });
});