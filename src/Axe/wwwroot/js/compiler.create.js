$(document).ready(function () {
    var countOfTestCases = 1; // Default value for countOfTestCases

    // This function determinates the current count of test cases.
    (function () {
        var i = 1;
        while ($('#input_' + i).length) {
            $('button[name = "btnDeleteTestCase_' + i + '"]').click(function () { deleteTestCase(this.value); });
            i++;
        }
        countOfTestCases = i;
    })();

    console.log('The current count of test cases: ' + countOfTestCases);

    $('#btnAddTestCase').click(function () {
        console.log('btnAddTestCase was clicked');

        $('<input>', {
            id: 'input_' + countOfTestCases,
            name: 'TestCases[' + countOfTestCases + "].Input",
            value: '',
            placeholder: 'input_' + countOfTestCases,
            class: 'form-control'
        }).appendTo($('#testCasesFormGroup'));

        $('<input>', {
            id: 'output_' + countOfTestCases,
            name: 'TestCases[' + countOfTestCases + "].Output",
            value: '',
            placeholder: 'output_' + countOfTestCases,
            class: 'form-control'
        }).appendTo($('#testCasesFormGroup'));

        $('<button>', {
            name: 'btnDeleteTestCase_' + countOfTestCases,
            value: countOfTestCases,
            type: 'button',
            class: 'btn btn-default',
            click: function () { deleteTestCase(this.value); }
        }).text('X').appendTo($('#testCasesFormGroup'));

        countOfTestCases++;
        console.log('The current count of test cases: ' + countOfTestCases);
    });

    function deleteTestCase(id) {
        console.log('deleteTestCase() was called for: ' + id);

        $('#input_' + id).remove();
        $('#output_' + id).remove();
        $('button[name = "btnDeleteTestCase_' + id + '"]').remove();

        if (id < countOfTestCases) {
            for (var i = +id + 1; i <= countOfTestCases; i++) {
                var newIndex = i - 1;

                $('#input_' + i).attr({
                    id: 'input_' + newIndex,
                    name: 'TestCases[' + newIndex + "].Input",
                    placeholder: 'input_' + newIndex
                });

                $('#output_' + i).attr({
                    id: 'output_' + newIndex,
                    name: 'TestCases[' + newIndex + "].Output",
                    placeholder: 'output_' + newIndex
                });

                $('button[name = "btnDeleteTestCase_' + i + '"]').attr({
                    name: 'btnDeleteTestCase_' + newIndex,
                    value: newIndex
                });
            }
        }

        countOfTestCases--;
        console.log('The current count of test cases: ' + countOfTestCases);
    }
});