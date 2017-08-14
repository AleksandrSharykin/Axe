$(document).ready(function () {
    var examForm = document.getElementById("examForm");
    if (examForm) {
        setInterval(function () {
            examForm.submit();
        }, 12000)
    }
});