var Form = $("#password-changing-form");
var AccountSettingsModal = $("#AccountSettings");

Form.submit(function (e) {
    e.preventDefault();
    var _ConfirmPassword = $("input[name='PasswordConfirmPassword']").val();
    var _Password = $("input[name='Password']").val();

    if (_ConfirmPassword.length <= 0) {
        Toast.fire()
        {
            Toast.fire({
                icon: 'warning',
                html: '<span class="font-weight-bold text-black-50 ml-1">Kimliğinizi doğrulamak için parolanızı girmelisiniz</span>'
            });
        }
    }
    else if (_Password.length < 7) {
        Toast.fire({
            icon: 'warning',
            html: '<span class="font-weight-bold text-black-50 ml-1">Yeni parola seçmelisiniz</span>'
        });
    }
    else {
        $.post("/api/password/post", { PasswordConfirmPassword: _ConfirmPassword, Password: _Password })
            .done(function (result) {
                if (result.status === true) {
                    $("input[name='PasswordConfirmPassword']").val('');
                    $("input[name='Password']").val('');

                    AccountSettingsModal.modal('hide');
                    Toast.fire({
                        icon: 'success',
                        html: '<span class="font-weight-bold text-black-50 ml-1">' + result.message + '</span>'
                    });
                }
                else {
                    if (result.message === 'Kimliğinizi doğrulayamadık: Onay parolası') {
                        $("input[name='PasswordConfirmPassword']").val('');
                        $("input[name='PasswordConfirmPassword']").focus();
                    }
                    Toast.fire({
                        icon: 'warning',
                        html: '<span class="font-weight-bold text-black-50 ml-1">' + result.message + '</span>'
                    });
                }
            });
    }
});
