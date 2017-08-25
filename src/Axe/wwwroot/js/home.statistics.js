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

    var exams =
        {
            url: '/api/statistics/GetExams',
            method: 'get',
            data: { periodStart: new Date(2017, 7, 12).toISOString(), periodEnd: new Date().toISOString() },
            dataType: 'json',
            success: function (data) {
                $('#exams').text(data.join('\n'));
            }
        };
    $.ajax(exams);
});