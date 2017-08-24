$(document).ready(function () {
    $('span[role=button]').click(function () {
        var id = $(this).attr('value');
        if (!id)
            return;
        //var request = new XMLHttpRequest();

        //request.onreadystatechange = function () {
        //    console.log('state = ', this.readyState);
        //    console.log('status = ', this.status);
        //    if (this.readyState == 4 && this.status == 200) {
        //        var data = this.responseText;
        //        console.log(data);
        //        console.log(typeof data);
        //    }
        //};

        //request.open('GET', '/Assessments/Item/' + id, true);
        //request.send();

        $.ajax({
            url: ('/Assessments/Item/' + id),
            method: 'get',
            dataType: 'json',
            success: function (data) {
                $('#TechnologyName').text(data.technologyName);
                $('#ExaminerName').text(data.examinerName);
                $('#ExamDate').text(data.examDate);

                if (data.isPassed != null) {
                    $('#ExamScore').show().text(data.examScore);
                    $('#ExamComment').show().text(data.examComment);
                    $('#IsPassed').show();

                    if (data.isPassed) {
                        $('#IsPassed').addClass('label-success').text('yes');
                    }
                    else {
                        $('#IsPassed').addClass('label-danger').text('no');
                    }
                }
                else {
                    $('#ExamScore').hide();
                    $('#ExamComment').hide();
                    $('#IsPassed').hide();
                }

                $('#ExamActions').html('');
                console.log(data);
                if (data.canEdit)
                    $('#ExamActions').append($('<a href="/Assessments/Input/' + id + '" class="btn btn-primary"> Edit </a>'));
                if (data.canMark)
                    $('#ExamActions').append($('<a href="/Assessments/Mark/' + id + '" class="btn btn-primary"> Mark </a>'));
                if (data.canDelete)
                    $('#ExamActions').append($('<a href="/Assessments/Delete/' + id + '" class="btn btn-default">Delete</a>'));
            },
            error: function (e) { console.log('error ' + e); }
        });

    });

});