$(document).ready(function () {
    $('#searchReset').click(function () {
        $('#questionsSearch').val(null).keyup();
    });
    var fSearch = function () {
        var filter = this.value.toLowerCase();
        var data = $('#questionsTable tr').each(function () {
            var txt = this['title'];
            if (!filter || txt.toLowerCase().indexOf(filter) >= 0)
                this.style.display = "";
            else
                this.style.display = "none";
        });
    };
    $('#questionsSearch').keyup(fSearch);

    var summator = function () {
        var score = 0;
        var txt = $('.question-score');
        for (var i = 0; i < txt.length; i++) {
            var tr = $(txt[i]).parents('tr')[0];
            var chk = $(tr).find('.question-selector');
            if (chk && $(chk).is(':checked')) {
                score += parseInt($(txt[i]).text());
            }
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

    $('.question-selector').change(summator);
    summator();
});