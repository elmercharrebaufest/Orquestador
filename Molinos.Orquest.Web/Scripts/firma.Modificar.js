$(document).ready(function () {

    if ($('.resultadoExitosa').val() == "Exitosa") {
        MostrarAlertaExitosa();
    }

    $("#aceptar").on("click", function () {
        $('.resultadoCancelada').val("");
        $('.resultadoExitosa').val("");
        $('#datos-form').submit();
    });
    $("#logoBoton").filestyle({ buttonName: "btn-primary", buttonText: "Buscar Logo", size: "sm", icon: false });
    $("#faviconBoton").filestyle({ buttonName: "btn-primary", buttonText: "Buscar Icono", size: "sm", icon: false });

});