$(document).ready(function () {
    $('.btn[data-toggle=modal]').click(function () {
        var tr = $(this).parents('tr').first();
        var id = tr.attr('value');

        if (!id)
            return;
        $.ajax({
            url: '/quiz/details/' + id,
            method: 'get',
            error: function (e) { console.log(e) },
            success: function (data) {
                $('#quizTitle').text(data.title);
                $('#quizJudge').text(data.judge);

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