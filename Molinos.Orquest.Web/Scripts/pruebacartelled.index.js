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

    $("#linkEjecutarIntervalo").click(function () {
        event.preventDefault();
        var url = $("#linkEjecutarIntervalo").attr("href") + "&texto=" + $('#texto').val() + "&numeroPrograma=" + $('#numeroPrograma').val() + "&numeroTrama=" + $('#numeroTrama').val() + "&numeroVariable=" + $('#numeroVariable').val() + "&textoSecundario=" + $('#textoSecundario').val() + "&intervalo=" + $('#intervalo').val();
        $("#linkEjecutarIntervalo").attr("disabled", "disabled");
        var div = $("#resultado-cartel-intervalo");
        div.html(div.data().textoCarga);
        $.get(url, function (resultado) {
            div.html(resultado);
            $("#linkEjecutarIntervalo").removeAttr("disabled");
        });
        return false;
    });


    $("#linkDetenerIntervalo").click(function () {
        event.preventDefault();
        var url = $("#linkDetenerIntervalo").attr("href");
        $("#linkDetenerIntervalo").attr("disabled", "disabled");
        var div = $("#resultado-cartel-intervalo");
        $.get(url, function (resultado) {
            div.html(resultado);
            $("#linkDetenerIntervalo").removeAttr("disabled");
        });
        return false;
    });
});

