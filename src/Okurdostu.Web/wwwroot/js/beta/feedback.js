var feedbackForm = $("#FeedbackForm");
feedbackForm.submit(function (e) {
    e.preventDefault();
    setTimeout(function () {
        if (!($("#Message-error").length || $("#Email-error").length)) {
            $('#spinner-div').show();
            var feedbackMessage = $("#FeedbackForm textarea[id='Message']");
            var feedbackEmail = $("#FeedbackForm input[id='Email']");

            $.post("/beta/feedback", { Message: feedbackMessage.val(), Email: feedbackEmail.val(), __RequestVerificationToken: validatetoken })
                .done(function (result) {
                    $("#Feedback").modal("hide");
                    feedbackMessage.val("").trigger("change");
                    $('#spinner-div').hide();
                    Toast.fire({
                        icon: "success",
                        html: "<span class=\"font-weight-bold text-black-50 ml-1\">" + result.message + "</span>"
                    });
                })
                .fail(function (result) {
                    $('#spinner-div').hide();
                    Toast.fire({
                        icon: "error",
                        html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
                    });
                });
        }
    }, 5);
});