var oldUniversityId, oldDepartment, oldActivitiesSocieties, oldStartYear, oldEndYear;

function getModalToEditEducation(id) {
    $.get("/api/me/educations/" + id).done(function (result) {
        if (result.succes === true) {
            oldUniversityId = result.data.universityId;
            oldActivitiesSocieties = result.data.activitiesSocieties;
            oldDepartment = result.data.department;
            oldStartYear = result.data.startYear;
            oldEndYear = result.data.endYear;

            $('#education-edit-modal-body').load("/education/editview/?ActivitiesSocieties=" + result.data.activitiesSocieties.replace(" ", "%20") + "&Department=" + result.data.department.replace(" ", "%20") + "&UniversityId=" + result.data.universityId + "&EducationId=" + result.data.educationId + "&StartYear=" + result.data.startYear + "&EndYear=" + result.data.endYear + "&AreUniversityorDepartmentCanEditable=" + result.data.areUniversityorDepartmentCanEditable);
            $('#education-edit-modal').modal('show');
        }
    });
};

var universityId, educationId, department, activitiesSocieties, startYear, endYear;
//edit
$(document).on('submit', '#edit-education-form', function (evt) {
    evt.preventDefault();
    universityId = $("#edit-education-form select[name=UniversityId]").val();
    department = $("#edit-education-form input[name=Department]").val();
    activitiesSocieties = $("#edit-education-form input[name=ActivitiesSocieties]").val();
    startYear = $("#edit-education-form select[name=StartYear]").val();
    endYear = $("#edit-education-form select[name=EndYear]").val();
    educationId = $("#edit-education-form input[name=EducationId]").val();

    if (department != null && department.length <= 0) {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1\">Bölümünüzü yazmalısınız</span>"
        });
    }
    else {
        if (universityId != oldUniversityId || department != oldDepartment || activitiesSocieties != oldActivitiesSocieties || startYear != oldStartYear || endYear != oldEndYear) {
            apiEducationPut();
        }
        else {
            Toast.fire({
                icon: "info",
                html: '<span class="font-weight-bold text-black-50 ml-1">Bir değişiklik yapmadınız</span>'
            });
        }
    }
});
//add
$('#add-education-form').submit(function (evt) {
    evt.preventDefault();
    universityId = $("#add-education-form select[name=UniversityId]").val();
    department = $("#add-education-form input[name=Department]").val();
    activitiesSocieties = $("#add-education-form input[name=ActivitiesSocieties]").val();
    startYear = $("#add-education-form select[name=StartYear]").val();
    endYear = $("#add-education-form select[name=EndYear]").val();
    educationId = $("#add-education-form input[name=EducationId]").val();

    if (department != null && department.length <= 0) {
        Toast.fire({
            icon: "info",
            html: '<span class="font-weight-bold text-black-50 ml-1">Bölümünüzü yazmalısınız</span>'
        });
    }
    else {
        apiEducationPost();
    }
});

function apiEducationPost() {
    $.post("/api/me/educations", { UniversityId: universityId, Department: department, ActivitiesSocieties: activitiesSocieties, StartYear: startYear, EndYear: endYear})
        .done(function (result) {
            $('#edit-education-button').prop('disabled', true);
            $('#add-education-button').prop('disabled', true);
            Toast.fire({
                icon: "success",
                html: "<span class=\"font-weight-bold text-black-50 ml-1\">" + result.message + "</span>"
            });
            setInterval(function () { location.reload() }, 2000);
        })
        .fail(function (result) {
            Toast.fire({
                icon: "warning",
                html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
            });
        });
};

function apiEducationPut() {
    $.ajax({
        url: '/api/me/educations/' + educationId,
        type: 'PUT',
        data: { UniversityId: universityId, Department: department, ActivitiesSocieties: activitiesSocieties, StartYear: startYear, EndYear: endYear},
        success: function (result) {
            $('#education-remove-modal').modal('hide');
            $('#education-' + _educationIdForRemove).attr('style', 'display:none;');
            Toast.fire({
                icon: "success",
                html: "<span class=\"font-weight-bold text-black-50 ml-1\">" + result.message + "</span>"
            });
            setInterval(function () { location.reload() }, 2000);
        },
        error: function (result) {
            Toast.fire({
                icon: "warning",
                html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
            });
        }
    });
};


//remove
var _educationIdForRemove;
function getModalToRemoveEducation(id) {
    $("#education-remove-modal").modal('show');
    _educationIdForRemove = id;
};

function removeEducation() {
    $.ajax({
        url: '/api/me/educations/' + _educationIdForRemove,
        type: 'PATCH',
        data: { Id: _educationIdForRemove },
        success: function (result) {
            $('#education-remove-modal').modal('hide');
            $('#education-' + _educationIdForRemove).attr('style', 'display:none;');
            Toast.fire({
                icon: "success",
                html: "<span class=\"font-weight-bold text-black-50 ml-1\">" + result.message + "</span>"
            });
        },
        error: function (result) {
            Toast.fire({
                icon: "warning",
                html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
            });
            setInterval(function () {
                location.reload();
            }, 2000);
        }
    });
};
//remove



var _educationIdForDocument;
var documentFile;

function getModalToConfirmFile(id) {
    $.get("/api/me/educations/" + id).done(function (result) {
        $("#education-confirm-modal").modal('show');
        _educationIdForDocument = id;
    });
}

function sendDocument() {
    var formData = new FormData();
    formData.append("File", $('#educationDocument')[0].files[0]);
    formData.append("Id", _educationIdForDocument);
    $("#spinner-div").show();
    $.ajax({
        type: "POST",
        url: "/api/me/educationdocuments/",
        data: formData,
        processData: false,
        contentType: false,
        success: function (result) {
            setTimeout(function () { $("#spinner-div").hide(); }, 250);
            Toast.fire({
                icon: "success",
                html: "<span class=\"font-weight-bold text-black-50 ml-1\">" + result.message + "</span>"
            });
            setTimeout(function () { location.reload(); }, 2000)
        },
        error: function (result) {
            setTimeout(function () { $("#spinner-div").hide(); }, 10);
            Toast.fire({
                icon: "warning",
                html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
            });
        }
    }
    );
}

$('#send-education-document').click(function () {
    documentFile = $('#educationDocument').prop('files')[0];

    if (documentFile != null) {
        sendDocument();
    }
    else {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1\">Dosya seçmelisiniz</span>"
        });
    }
});