var titleForm = $("#title-form");
titleForm.submit(function(e){
    e.preventDefault();

    var newTitle = $("#Title").val();
    var titleError = $("#Title-error").html();

    if(typeof titleError != "undefined"){
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + titleError + "</span>"
        });
    }
    else if (newTitle == oldTitle)
    {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1 \">Hiç bir değişiklik yapmadınız</span>"
        });
    }
    else
    {
        $.ajax({
            url: "/api/me/needs/title",
            type: 'Patch',
            data: { Title: newTitle, Id: Id},
            success: function (result) {
                $("#edittitle").modal("hide");
                Toast.fire({
                    icon: "success",
                    html: "<span class=\"font-weight-bold text-black-50 ml-1\">" + result.message + "</span>"
                });
                setInterval(() => {
                    window.location.href = "/" + result.data.link;
                }, 1000);
            },
            error: function (result) {
                Toast.fire({
                    icon: "warning",
                    html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
                });
            }
        });

    }

});