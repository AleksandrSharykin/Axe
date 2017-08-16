$(document).ready(function () {
    var summator = function () {
        var score = 0;
        var txt = $('.answer-score');
        for (var i = 0; i < txt.length; i++) {
            score += parseInt($(txt[i]).val());
        }

        if (!score) {
            score = 0;
            $('#totalScore').addClass('text-danger');
        }
        else {
            $('#totalScore').removeClass('text-danger');
        }

        $('#totalScore').text(score);
    };

    $('.answer-score').change(summator);
    summator();
})