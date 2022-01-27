$(function () {
    $("#link-humedad").click(function () {
        event.preventDefault();

        var url = $("#link-humedad").attr("href") + "&material=" + $('#materialId').val();
        $("#link-humedad").attr("disabled", "disabled");
        var div = $("#resultado-humedad");
        div.html(div.data().textoCarga);
        $.get(url, function (resultado) {
            div.html(resultado);
            $("#link-humedad").removeAttr("disabled");
        });
        return false;
    });
});