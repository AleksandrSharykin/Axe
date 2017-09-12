/// <reference path="../typings/jquery/jquery.d.ts" />

console.log('hello ts');

let test: number = 314;

window.onload =  function () {
    console.log('page loaded');
    console.log(test)

    $(document).click(function () {
        alert('ping')
   })
    console.log($(document).length);
}

let matrix: number[][] = [ [1,2], [3,4] ];


console.log(matrix.join('\n'))