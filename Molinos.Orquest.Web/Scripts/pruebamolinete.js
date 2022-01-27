$(function () {
    $("#pruebaLoop").hide();

    $("#btn-recargar").click(function () {
        BlockUI();
    });

    $(".no-conectado").addClass("disabled").attr("disabled", "disabled");

    var notificador = $.connection.notificarLectura;

    notificador.client.actualizarLecturaTarjeta = function (lectura) {
        $("#Tarjetaentrada").val("");
        $("#Tarjetasalida").val("");
        $("#Tarjeta" + lectura.Sentido).val(lectura.Tarjeta);
        setTimeout(function () {
            $("#Tarjetaentrada").val("");
            $("#Tarjetasalida").val("");
        }, 5000);
    };

    notificador.client.actualizarTransitoMolinete = function (lectura) {

        limpiarIndicador();

        let estadoTransito =
            (lectura.Direccion != "null" && lectura.Denegado == false) ? "transito" :
                (lectura.Direccion == "null" && lectura.Denegado == false) ? "sintransito" :
                    (lectura.Direccion != "null" && lectura.Denegado == true) ? "denegado" : "Estado no especificado";


        if (lectura.Direccion != "null") {

            $('#Giro' + lectura.Direccion)
                .removeClass("badge-molinete-unknown")
                .addClass("badge-molinete-" + estadoTransito);

        } else {
            $('#Giroentrada')
                .removeClass("badge-molinete-unknown")
                .addClass("badge-molinete-" + estadoTransito);

            $('#Girosalida')
                .removeClass("badge-molinete-unknown")
                .addClass("badge-molinete-" + estadoTransito);
        }


        setTimeout(function () {
            limpiarIndicador();
        }, 5000);
    };


    notificador.client.actualizarLecturaDni = function (lectura) {
        $("#LecturaDni").val("");
        $("#LecturaDni").val(lectura.QR);

        if (lectura.Lector == "entrada") {
            $('#Lectorentrada')
                .removeClass("badge-molinete-unknown")
                .addClass("badge-molinete-transito");
        } else {
            $('#Lectorsalida')
                .removeClass("badge-molinete-unknown")
                .addClass("badge-molinete-transito");
        }
        setTimeout(function () {

            $('#Lectorentrada').removeClass("badge-molinete-transito");
            $('#Lectorentrada').addClass("badge-molinete-unknown");

            $('#Lectorsalida').removeClass("badge-molinete-transito");
            $('#Lectorsalida').addClass("badge-molinete-unknown");

            $("#LecturaDni").val("");
        }, 5000);
    };

    //suscribirser al Hub
    $.connection.hub.start().done(function () {
        notificador.server.escucharMolinete($('#pruebaMolinete').data().codigoMolinete);
    });

    $(".boton-barrera").click(function () {
        var self = $(this);
        $.getJSON(self.data().urlActivacion, function (resultado) {
            if (resultado.Codigo == 0) {
                MostrarAlertaExitosa(resultado.Mensaje);
            } else {
                MostrarAlertaError(resultado.Mensaje);
            }
        });
    });

    $(window).on('beforeunload', function () {
        $.getJSON($('#pruebaMolinete').data().urlDesuscribir, function () {
        });
    });

    var divErrores = $('#alerta-error-itc');
    if (divErrores.data().cantidadErrores > 0) {
        $("#alertaAdvertencia-body h4").text(divErrores[0].dataset.errorGenerico);
        $("#alertaAdvertencia-body").append(divErrores[0]);
        $("#alertaAdvertencia").modal('show', {
            keyboard: false
        });
    }

    //consultarEstado();
});

function limpiarIndicador() {

    $('#Giroentrada').removeClass("badge-molinete-transito");
    $('#Giroentrada').removeClass("badge-molinete-sintransito");
    $('#Giroentrada').removeClass("badge-molinete-denegado");
    $('#Giroentrada').addClass("badge-molinete-unknown");

    $('#Girosalida').removeClass("badge-molinete-transito");
    $('#Girosalida').removeClass("badge-molinete-sintransito");
    $('#Girosalida').removeClass("badge-molinete-denegado");
    $('#Girosalida').addClass("badge-molinete-unknown");
}