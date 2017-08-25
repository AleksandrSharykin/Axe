$(document).ready(function () {
    var memberscount =
        {
            url: '/api/statistics/getmemberscount',
            method: 'get',
            success: function (data) {
                $('#total').text(data);
            }
        };
    $.ajax(memberscount);

    var questions = {
        url: '/api/statistics/getcomplexquestions',
        method: 'get',
        success: function (data) {
            var formatter = function (item, prop) {
                var value = item[prop];
                if (prop === 'id')
                    return '<a href="/questions/details/' + value + '">' + value + '</a>';

                if (prop === 'preview')
                    return '<span class="codeblock">' + value + '</span>';

                return value;
            };
            var rows = data.map(function (o) { return toTr(o, formatter); }).join('');

            $('#questions').html(rows);
        },
        error: function (e) { console.log(e) }
    };
    $.ajax(questions);

    var d = new Date();
    $('#periodEnd').val($.datepicker.formatDate('mm/dd/yy', d))
    d.setDate(d.getDate() - 7);
    $('#periodStart').val($.datepicker.formatDate('mm/dd/yy', d))

    $('#showExams').click(showExams);
    showExams();

    function showExams() {
        var firstDay = $('#periodStart').val();
        if (firstDay)
            firstDay = $.datepicker.parseDate('mm/dd/yy', firstDay)
        else
            return;

        var lastDay = $('#periodEnd').val();
        if (lastDay)
            lastDay = $.datepicker.parseDate('mm/dd/yy', lastDay)
        else
            return;

        if (!firstDay || !lastDay)
            return;

        if (firstDay > lastDay) {
            var d = firstDay;
            firstDay = lastDay;
            lastDay = d;
        }

        var exams =
            {
                url: '/api/statistics/getexams',
                method: 'get',
                data: { periodStart: firstDay.toISOString(), periodEnd: lastDay.toISOString() },
                dataType: 'json',
                success: function (data) {

                    $('#exams').html(data
                        .map(function (item) { return '<tr><td>' + item.day + '</td><td>' + item.count + '</td>' })
                        .join(''));
                }
            };
        $.ajax(exams);
    };

    function toTr(item, formatter) {
        return '<tr>'
            + Object.getOwnPropertyNames(item)
                .map(function (prop) { return '<td>' + formatter(item, prop) + '</td>'; })
                .join('')
            + '</tr>';
    }

});