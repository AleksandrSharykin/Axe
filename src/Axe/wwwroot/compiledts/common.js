/// <reference path="../typings/jquery/jquery.d.ts" />
console.log('hello ts');
var test = 314;
window.onload = function () {
    console.log('page loaded');
    console.log(test);
    $(document).click(function () {
        alert('ping');
    });
    console.log($(document).length);
};
var matrix = [[1, 2], [3, 4]];
console.log(matrix.join('\n'));
//# sourceMappingURL=common.js.map