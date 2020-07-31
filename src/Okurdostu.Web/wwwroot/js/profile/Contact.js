var Form = $("#contact-form");

Form.submit(function (e) {
    e.preventDefault();

    var _Twitter = $("input[name='Twitter']").val();
    var _Github = $("input[name='Github']").val();
    var _ContactEmail = $("input[name='ContactEmail']").val();

    $.ajax({
        url: '/api/me/contact',
        type: 'PATCH',
        data: { Twitter: _Twitter, Github: _Github, ContactEmail: _ContactEmail },
        success: function (result) {
            Toast.fire({
                icon: "success",
                html: "<span class=\"font-weight-bold text-black-50 ml-1\">" + result.message + "</span>"
            });
            setInterval(function () {
                location.reload();
            }, 2000);
        },
        error: function (result) {
            Toast.fire({
                icon: "warning",
                html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
            });
        }
    });
});