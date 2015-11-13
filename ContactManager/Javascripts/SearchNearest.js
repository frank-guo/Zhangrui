
$(document).ready(function () {
    var jsonData;

    $('#contactsForm').submit(function (event) {
        event.preventDefault();
    });

    $('#searchBtn').click(function (event) {
        if (navigator.geolocation) {
            var timeoutVal = 10 * 1000 * 1000;
            navigator.geolocation.getCurrentPosition(
              createJsonData,
              createJsonDataWithoutCoord,
              { enableHighAccuracy: true, timeout: timeoutVal, maximumAge: 0 }
            );
        }
        else {
            alert("Geolocation is not supported by this browser");
        }

        $.ajax({
            url: '/AllContacts/Index',
            data: jsonData,
            type: 'POST',
            contentType: 'application/json; charset=utf-8'
        });
    });

    function createJsonData(position) {
        var filter = {
            "FirstName": $('#firstName').val(),
            "LastName": $("#lastName").val(),
            "EmailAddress": $("#emailAddress").val(),
            "MyLatitude": position.coords.latitude,
            "MyLongitude": position.coords.longitude
        };
        jsonData = JSON.stringify(filter);
    }

    function createJsonDataWithoutCoord(error) {
        var filter = {
            "FirstName": $('#firstName').val(),
            "LastName": $("#lastName").val(),
            "EmailAddress": $("#emailAddress").val()
        };
        jsonData = JSON.stringify(filter);

        var errors = {
            1: 'Permission denied',
            2: 'Position unavailable',
            3: 'Request timeout'
        };
        alert("Error: " + errors[error.code]);
    }
});