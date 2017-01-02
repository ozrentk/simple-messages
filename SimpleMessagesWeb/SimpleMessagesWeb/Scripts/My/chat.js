require(["jquery", "jscookie", "bootstrap"], function ($, jscookie, bs) {
    $(function () {
        $('#conversation-area').scrollTop(1E10);

        var jqxhr =
            $.ajax({
                type: 'GET',
                url: 'http://localhost:14221/user?search=oz',
                //xhrFields: {
                //    withCredentials: true
                //},
                //beforeSend: function (xhr) {
                //    xhr.setRequestHeader("Authorization", "Basic " + base64token);
                //},
                crossDomain: true,
                success: function (data) {
                    var optionList = $.map(data, function (item) {
                        return $('<option/>', { text: item });
                    })
                    $("#user-list").append(optionList);
                }
            })


        //jscookie.set('neki-pero', 'Ždero');

        //var x = jscookie.get('neki-pero');

        //console.log(x);

        //$('#myStateButton').on('click', function () {
        //    $(this).button('complete') // button text will be "finished!"
        //})
    });
});