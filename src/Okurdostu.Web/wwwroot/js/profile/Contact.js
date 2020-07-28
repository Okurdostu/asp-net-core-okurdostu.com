var Form = $("#contact-form");

Form.submit(function (e) {
    e.preventDefault();

    var _Twitter = $("input[name='Twitter']").val();
    var _Github = $("input[name='Github']").val();
    var _ContactEmail = $("input[name='ContactEmail']").val();

    $.post("/api/me/contact", { Twitter: _Twitter, Github: _Github, ContactEmail: _ContactEmail })
        .done(function (result) {
            Toast.fire({
                icon: 'success',
                html: '<span class="font-weight-bold text-black-50 ml-1">' + result.message + '</span>'
            });
            setInterval(function () {
                location.reload();
            }, 2000);

        }).fail(function (result) {
            Toast.fire({
                icon: 'error',
                html: '<span class="font-weight-bold text-black-50 ml-1">' + result.responseJSON.message + '</span>'
            });
        });
});