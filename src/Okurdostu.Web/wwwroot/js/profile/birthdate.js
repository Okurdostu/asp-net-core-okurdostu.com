var months = ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"];
var dropdownYear = $("#Year");
var dropdownMonth = $("#Month");
var dropdownDay = $("#Day");
var Privacy = $("#Privacy");

if (oldSecretLevel != "undefined") {
    $(`option[value="${oldSecretLevel}"]`).attr("selected", "selected");
}

for (var m = 0; m < months.length - 1; m++) {
    if (oldMonth != "undefined" && m + 1 === oldMonth) {
        dropdownMonth.append($(`<option selected value="${m + 1}">${months[m]}</option>`));
    }
    else {
        dropdownMonth.append($(`<option value="${m + 1}">${months[m]}</option>`));
    }
}
for (var i = 2010; i >= 1923; i--) {
    if (oldYear != "undefined" && i === oldYear) {
        dropdownYear.append($(`<option selected value="${i}">${i}</option>`));
    }
    else {
        dropdownYear.append($(`<option value="${i}">${i}</option>`));
    }
}

function getDays() {
    dropdownDay.html("");
    selectedMonth = dropdownMonth.val();
    selectedYear = dropdownYear.val();
    if (selectedMonth == 0 || selectedMonth == 2 || selectedMonth == 4 || selectedMonth == 6 || selectedMonth == 7 || selectedMonth == 9 || selectedMonth == 11) {
        for (var d = 1; d <= 31; d++) {
            if (oldDay != "undefined" && d === oldDay) {
                dropdownDay.append($(`<option selected value="${d}">${d}</option>`));
            }
            else {
                dropdownDay.append($(`<option value="${d}">${d}</option>`));
            }
        }
    }
    else if (selectedMonth == 3 || selectedMonth == 5 || selectedMonth == 9 || selectedMonth == 10) {
        for (var d = 1; d <= 30; d++) {
            if (oldDay != "undefined" && d === oldDay) {
                dropdownDay.append($(`<option selected value="${d}">${d}</option>`));
            }
            else {
                dropdownDay.append($(`<option value="${d}">${d}</option>`));
            }
        }
    }
    else {
        if (selectedYear % 4 === 0) {
            for (var d = 1; d <= 29; d++) {
                if (oldDay != "undefined" && d === oldDay) {
                    dropdownDay.append($(`<option selected value="${d}">${d}</option>`));
                }
                else {
                    dropdownDay.append($(`<option value="${d}">${d}</option>`));
                }
            }
        }
        else {
            for (var d = 1; d <= 28; d++) {
                if (oldDay != "undefined" && d === oldDay) {
                    dropdownDay.append($(`<option selected value="${d}">${d}</option>`));
                }
                else {
                    dropdownDay.append($(`<option value="${d}">${d}</option>`));
                }
            }
        }
    }
}
getDays();
dropdownMonth.change(function () {
    getDays();
});
dropdownYear.change(function () {
    getDays();
});

var birtDateForm = $("#BirthDateForm")

birtDateForm.submit(function (e) {
    e.preventDefault();
    $.ajax({
        url: '/api/me/birthdate',
        type: 'PATCH',
        data: { Year: dropdownYear.val(), Month: dropdownMonth.val(), Day: dropdownDay.val(), BDSecretLevel: Privacy.val() },
        success: function (result) {
            Toast.fire({
                icon: "success",
                html: "<span class=\"font-weight-bold text-black-50 ml-1\">" + result.message + "</span>"
            });
            setInterval(function () { location.reload() }, 1000);
        },
        error: function (result) {
            Toast.fire({
                icon: "warning",
                html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
            });
        }
    });

});
