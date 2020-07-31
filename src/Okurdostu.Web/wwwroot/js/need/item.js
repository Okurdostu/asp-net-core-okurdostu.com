function AddItem(Id) {
    var inputLink = $("#linkInput").val();

    if (inputLink.length > 0) {
        $.post("/api/needs/item", { itemLink: inputLink, needId: Id }).done(function (result) {
            location.reload();
        }).fail(function (result) {
            Toast.fire({
                icon: 'error',
                html: '<span class="font-weight-bold text-black-50 ml-1">' + result.responseJSON.message + '</span>'
            });
        });
    }
    else {
        Toast.fire({
            icon: 'info',
            html: '<span class="font-weight-bold text-black-50 ml-1">' + 'Hedefinizi belirlemek için ona dair bir internet adresi vermelisiniz' + '</span>'
        });
    }
}
