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

        console.log(firstDay + ' ' + lastDay);

        if (!firstDay || !lastDay)
            return;

        if (firstDay > lastDay) {
            var d = firstDay;
            firstDay = lastDay;
            lastDay = d;
        }

        var exams =
            {
                url: '/api/statistics/GetExams',
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
});