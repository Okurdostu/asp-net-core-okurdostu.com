var oldTitle = "";
var oldDescription = "";
var Id;

function getInformation(id){
    oldTitle = $("#Title").val();
    oldDescription = $("#Description").val();
    Id = id;
}

function sendToConfirmation(id) {
    $("#spinner-div").show();
    $.ajax({
        url: "/api/me/needs/SendToConfirmation",
        type: 'Patch',
        data: { Id: id },
        success: function () { location.reload(); },
        error: function (result) {
            setTimeout(function () { $("#spinner-div").hide(); }, 500);
            Toast.fire({
                icon: "error",
                html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
            });
        }
    });
}