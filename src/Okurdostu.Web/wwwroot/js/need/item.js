function AddItem(Id) {
    var inputLink = $("#linkInput").val();
    var button = $(".btn.btn-outline-purple.rounded-custom.w-100.font-weight-bold");

    if (inputLink.includes("amazon.com.tr") || inputLink.includes("pandora.com.tr") || inputLink.includes("udemy")) {
        button.attr("disabled", "true");
        $("#spinner-div").show();
        $.post("/api/needs/item", { itemLink: inputLink, needId: Id, __RequestVerificationToken: validatetoken })
            .done(function () { location.reload(); })
            .fail(function (result) {
                button.removeAttr("disabled");
                setTimeout(function () { $("#spinner-div").hide(); }, 500);
                Toast.fire({
                    icon: "error",
                    html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
                });
            });
    }
    else {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1\">Hedefinizi belirlemek için ona dair bir internet adresi vermelisiniz</span>"
        });
    }
}
