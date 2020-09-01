var descriptionForm = $("#description-form");
descriptionForm.submit(function(e){
    console.log(Id);
    e.preventDefault();

    var newDescription = $("#Description").val();
    var descriptionError = $("#Description-error").html();

    if(typeof descriptionError != "undefined"){
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + descriptionError + "</span>"
        });
    }
    else if (newDescription == oldDescription)
    {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1 \">Hiç bir değişiklik yapmadınız</span>"
        });
    }
    else
    {
        $.ajax({
            url: "/api/needs/description",
            type: 'Patch',
            data: { Description: newDescription, Id: Id},
            success: function (result) {
                $("#editdescription").modal("hide");

                Toast.fire({
                    icon: "success",
                    html: "<span class=\"font-weight-bold text-black-50 ml-1\">" + result.message + "</span>"
                });

                $("p#description").html(result.data.description);
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