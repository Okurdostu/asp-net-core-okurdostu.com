var Form = $("#username-changing-form");
var AccountSettingsModal = $("#AccountSettings");

Form.submit(function (e) {
    e.preventDefault();
    var _ConfirmPassword = $("input[name='UsernameConfirmPassword']").val();
    var _Username = $("input[name='Username']").val();

    if (_ConfirmPassword.length <= 0) {
        Toast.fire()
        {
            Toast.fire({
                icon: "info",
                html: "<span class=\"font-weight-bold text-black-50 ml-1\">Kimliğinizi doğrulamak için parolanızı girmelisiniz</span>"
            });
        }
    }
    else if (_Username.length < 3) {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1\">En az 3 karakterli: yeni bir kullanıcı adı seçmelisiniz</span>"
        });
    }
    else {
        $.ajax({
            url: '/api/me/username',
            type: 'PATCH',
            data: { UsernameConfirmPassword: _ConfirmPassword, Username: _Username },
            success: function (result) {
                $('#change-username-button').prop('disabled', true);
                $("input[name='Username']").prop('disabled', true);
                Toast.fire({
                    icon: "success",
                    html: "<span class=\"font-weight-bold text-black-50 ml-1\">" + result.message + "</span>"
                });
                setInterval(function () {
                    window.location.href = '/' + result.data;
                }, 2000);
            },
            error: function (result) {
                if (result.responseJSON.message === "Kimliğinizi doğrulayamadık: Onay parolası") {
                    $("input[name='UsernameConfirmPassword']").focus();
                }
                Toast.fire({
                    icon: "warning",
                    html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
                });
            }
        });
    }
});
