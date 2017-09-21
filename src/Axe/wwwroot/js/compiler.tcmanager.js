/*
    This script is used to add new item of test case into form
*/
$(document).ready(function () {
    var countOfTestCases = 1; // Default value for countOfTestCases

    // This function determinates the current count of test cases.
    (function () {
        var i = 1;
        while ($('#input_' + i).length) {
            $('button[name = "btnDeleteTestCase_' + i + '"]').click(function () { deleteTestCase(this.value); });
            ++i;
        }
        countOfTestCases = i;
    })();

    console.log('The current count of test cases: ' + countOfTestCases);

    $('#btnAddTestCase').click(function () {
        console.log('btnAddTestCase was clicked');

        var div_group = $('<div>').attr({
            id: 'group_' + countOfTestCases.toString(),
            class: 'row',
        });
       
        var div = $('<div>').attr({
            class: 'form-group col-md-8'
        });

        var span = $('<div>').attr({
            class: 'col-md-1'
        }).append($('<span>').attr({
            id: 'span_' + countOfTestCases,
            class: 'badge badge-secondary'
        }).text((countOfTestCases + 1).toString()));

        var labelInput = $('<label>', {
            id: 'inputLabel_' + countOfTestCases,
            for: 'input_' + countOfTestCases,
            class: 'col-md-1'
        }).text('Input');

        var input = ($('<div>').attr({
            class: 'col-md-4'
        })).append($('<input>', {
            id: 'input_' + countOfTestCases,
            name: 'TestCases[' + countOfTestCases + "].Input",
            value: '',
            placeholder: 'input_' + countOfTestCases,
            class: 'form-control'
        }));

        var labelOutput = $('<label>', {
            id: 'outputLabel_' + countOfTestCases,
            for: 'output_' + countOfTestCases,
            class: 'col-md-1'
        }).text('Output');

        var output = ($('<div>').attr({
            class: 'col-md-4'
        })).append($('<input>', {
            id: 'output_' + countOfTestCases,
            name: 'TestCases[' + countOfTestCases + "].Output",
            value: '',
            placeholder: 'output_' + countOfTestCases,
            class: 'form-control'
        }));

        var button = ($('<div>', {
            class: 'col-md-1'
        })).append($('<button>', {
            name: 'btnDeleteTestCase_' + countOfTestCases,
            value: countOfTestCases,
            type: 'button',
            class: 'btn btn-danger col-md-12',
            click: function () { deleteTestCase(this.value); }
        }).text('X'));

        div.append(span, labelInput, input, labelOutput, output, button);
        
        div_group.append(div);
        div_group.appendTo($('#testCasesFormGroup'));
        
        countOfTestCases++;
        console.log('The current count of test cases: ' + countOfTestCases);
    });

    function deleteTestCase(id) {
        console.log('deleteTestCase() was called for: ' + id);
        
        $('#group_' + id).remove();

        if (id < countOfTestCases) {
            for (var i = +id + 1; i <= countOfTestCases; i++) {
                var newIndex = i - 1;

                $('#group_' + i).attr({
                    id: 'group_' + newIndex,
                });
                $('#span_' + i).attr({
                    id: 'span_' + newIndex,
                }).text(i);

                $('#inputLabel_' + i).attr({
                    id: 'inputLabel_' + newIndex,
                    for: 'input_' + newIndex,
                });

                $('#input_' + i).attr({
                    id: 'input_' + newIndex,
                    name: 'TestCases[' + newIndex + "].Input",
                    placeholder: 'input_' + newIndex
                });

                $('#outputLabel_' + i).attr({
                    id: 'outputLabel_' + newIndex,
                    for: 'output_' + newIndex,
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