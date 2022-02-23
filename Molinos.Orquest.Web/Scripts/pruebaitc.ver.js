function consultarEstado() {
    var estadoItc = $("#estadoItc");
    $.getJSON(estadoItc.data().urlEstado, { codigoDispositivo: estadoItc.data().codigo }, function (resultado) {
        estadoItc.html(resultado.Estado);
        estadoItc.attr("title", resultado.Mensaje);
        if (resultado.Conectado) {
            estadoItc.addClass("badge-success");
            estadoItc.removeClass("badge-danger");
        } else {
            estadoItc.addClass("badge-danger");
            estadoItc.removeClass("badge-success");
        }
    }).done(function () {
        setTimeout(consultarEstado, 2000);
    });
}

var verLogs = [];
$(function () {
    $("#btn-recargar").click(function () {
        BlockUI();
    });
    
    $(".no-conectado").addClass("disabled").attr("disabled", "disabled");

    var notificador = $.connection.notificarLectura;

    notificador.client.actualizarLecturaTarjeta = function (lectura) {
        $('#' + lectura.CodigoDispositivo).val(lectura.Valor);
    };
    
    notificador.client.actualizarLecturaEntrada = function (lectura) {
        $('#' + lectura.CodigoDispositivo)
            .removeClass("badge-entrada-itc-unknown")
            .toggleClass("badge-entrada-itc", lectura.Valor);
        setTimeout(function () {
            $('#' + lectura.CodigoDispositivo).removeClass("badge-entrada-itc");
        }, 5000);
    };

    notificador.client.actualizarLecturaQr = function (lectura) {
        $('#' + lectura.CodigoDispositivo).val(lectura.QR);

        setTimeout(function () {
            $('#' + lectura.CodigoDispositivo).val("");
        }, 5000);
    };

    notificador.client.actualizarEstadoDispositivo = function (estado) {
        if (!estado.Error) {
            MostrarAlertaExitosa("Conexión restablecida con el dispositivo ITC");
        } else {
            MostrarAlertaError("Falló conexión con el dispositivo ITC: " + estado.Mensaje);
        }
    };

    window.hubReady.done(function () {
        notificador.server.escucharItc($('#pruebaItc').data().codigoItc);
    })

    $(".boton-barrera").click(function() {
        var self = $(this);
        $.getJSON(self.data().urlActivacion, function(resultado) {
            if (resultado.Codigo == 0) {
                MostrarAlertaExitosa(resultado.Mensaje);
            } else {
                MostrarAlertaError(resultado.Mensaje);
            }
        });
    });

    $(".boton-tag").click(function () {
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
        $.getJSON($('#pruebaItc').data().urlDesuscribir, function() {
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

    consultarEstado();
});