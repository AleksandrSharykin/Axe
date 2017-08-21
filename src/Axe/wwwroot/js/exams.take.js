$(document).ready(function () {
    var examForm = document.getElementById("examForm");
    if (examForm) {
        // submitting intermediate values in background every 12 sec for monitoring
        setInterval(function () {
            var d = $('#examForm').serialize();

            $.ajax({
                url: ('/Exams/Monitor'),
                type: 'POST',
                dataType: 'json',
                data: d
            })

            //var xq = new XMLHttpRequest();
            //xq.open('post', '/Exams/Monitor', true);
            //xq.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
            //xq.send(d);

        }, 12000)
    }
});