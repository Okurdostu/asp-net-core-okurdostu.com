$(document).ready(function () {
    $("#show_hide_password").on('click', function (event) {
        event.preventDefault();
        if ($('#Password').attr("type") == "text") {
            $('#Password').attr('type', 'password');
            $('#eye').addClass("fa-eye-slash");
            $('#eye').removeClass("fa-eye");
        } else if ($('#Password').attr("type") == "password") {
            $('#Password').attr('type', 'text');
            $('#eye').removeClass("fa-eye-slash");
            $('#eye').addClass("fa-eye");
        }
    });
});
