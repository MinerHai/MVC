function showToast(message) {
    $.get('/Home/RenderToast', { message: message }, function (data) {
        $('body').append(data);
        $('.toast').fadeIn(200, function () {
            $(this).delay(2600).fadeOut(200, function () {
                $(this).remove();
            });
        });
    });
}