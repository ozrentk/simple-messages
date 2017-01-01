require(["jquery", "jscookie", "bootstrap"], function ($, jscookie, bs) {
    $(function () {
        jscookie.set('neki-pero', 'Ždero');

        var x = jscookie.get('neki-pero');

        console.log(x);

        $('#myStateButton').on('click', function () {
            $(this).button('complete') // button text will be "finished!"
        })
    });
});