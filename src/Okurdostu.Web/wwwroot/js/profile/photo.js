function FileSelect() {
    document.getElementById("photo-input").click();
}
function PhotoSelected() {
    var formData = new FormData();
    formData.append("File", $("#photo-input").prop("files")[0]);
    $("#photo-input").val("");
    $.ajax({
        type: "PATCH",
        url: "/api/me/photo/",
        data: formData,
        processData: false,
        contentType: false,
        success: function (result) {
            $("#profile-photo-modal").modal("hide");
            $("#profile-photo-img").attr({ "src": result.data.photo });
        },
        error: function (result) {
            Toast.fire({
                icon: "warning",
                html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
            });
        }
    }
    );
}
function RemovePhoto() {
    if ($("#profile-photo-img").attr("src") !== "/svg/user.svg") {
        $.ajax({
            type: "PATCH",
            url: "/api/me/photo/remove",
            success: function () {
                $("#profile-photo-modal").modal("hide");
                $("#profile-photo-img").attr({ "src": "/svg/user.svg" });
            },
            error: function (result) {
                Toast.fire({
                    icon: "warning",
                    html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
                });
            }
        }
        );
    }
    else {
        $("#profile-photo-modal").modal("hide");
    }
}