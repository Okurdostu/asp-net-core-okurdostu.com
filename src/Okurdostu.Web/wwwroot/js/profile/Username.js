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
                icon: 'warning',
                html: '<span class="font-weight-bold text-black-50 ml-1">Kimliğinizi doğrulamak için parolanızı girmelisiniz</span>'
            });
        }
    }
    else if (_Username.length < 3) {
        Toast.fire({
            icon: 'warning',
            html: '<span class="font-weight-bold text-black-50 ml-1">Yeni kullanıcı adı seçmelisiniz</span>'
        });
    }
    else {
        $.post("/api/me/username", { UsernameConfirmPassword: _ConfirmPassword, Username: _Username })
            .done(function (result) {
                $('#change-username-button').prop('disabled', true);
                $("input[name='Username']").prop('disabled', true);
                Toast.fire({
                    icon: 'success',
                    html: '<span class="font-weight-bold text-black-50 ml-1">' + result.message + '</span>'
                });
                setInterval(function () {
                    window.location.href = '/' + result.data;
                }, 2000);
            })
            .fail(function (result) {
                if (result.responseJSON.message === 'Kimliğinizi doğrulayamadık: Onay parolası') {
                    $("input[name='UsernameConfirmPassword']").focus();
                }
                Toast.fire({
                    icon: 'warning',
                    html: '<span class="font-weight-bold text-black-50 ml-1">' + result.responseJSON.message + '</span>'
                });
            });
    }
});
