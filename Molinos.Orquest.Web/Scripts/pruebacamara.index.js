$(function () {

    if ($('#imagen-patente').length > 0) {
        TomarFotoConPatente();

        $("#link-camara").click(function () {
            TomarFotoConPatente();
            return false;
        });
    }
    else
    {
        $("#link-camara").click(function () {
            var imagenCamara = $("#imagen-camara");
            imagenCamara.attr("src", imagenCamara.data().urlFoto + "&timestamp=" + new Date().getTime());
            return false;
        });
    }
    

});


function TomarFotoConPatente() {
    var codigo = $("#Dispositivo_Codigo").val();
    $.getJSON($("#links").data().urlObtenerPatente, { codigo: codigo }, function (data) {
        $("#patente").text(data.patente);
        $('#imagen-patente').attr('src', data.imagen);
    }).done(function () {
    });
}