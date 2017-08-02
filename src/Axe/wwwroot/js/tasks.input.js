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
});