$(function () {
    $("#link").click(function () {
        event.preventDefault();

        var url = $("#link").attr("href") + "&texto=" + $('#texto').val() + "&numeroPrograma=" + $('#numeroPrograma').val() + "&numeroTrama=" + $('#numeroTrama').val() + "&numeroVariable=" + $('#numeroVariable').val();
        $("#link").attr("disabled", "disabled");
        var div = $("#resultado-humedad");
        div.html(div.data().textoCarga);
        $.get(url, function (resultado) {
            div.html(resultado);
            $("#link").removeAttr("disabled");
        });
        return false;
    });
});

