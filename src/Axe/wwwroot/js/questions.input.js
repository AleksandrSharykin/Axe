$(document).ready(function () {

    if ($('#decoratedText')) {
        var mdParse = function () {
            var content = $('#rawText').val();
            $('#decoratedText').html(md2html(content));
        };

        $('#rawText').change(mdParse);
        mdParse();
    }

    var summator = function () {
        var score = 0;
        var txt = $('.answer-score');
        for (var i = 0; i < txt.length; i++) {
            score += parseInt($(txt[i]).val());
        }

        if (!score) {
            score = 0;
            $('#totalScore').removeClass('label-primary');
            $('#totalScore').addClass('label-danger');
        }
        else {
            $('#totalScore').removeClass('label-danger');
            $('#totalScore').addClass('label-primary');
        }

        $('#totalScore').text(score);
    };

    $('.answer-score').change(summator);
    summator();
})