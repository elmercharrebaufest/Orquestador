function consultarEstado() {
    var estadoItc = $("#estadoItc");
    $.getJSON(estadoItc.data().urlEstado, { codigoDispositivo: estadoItc.data().codigo }, function (resultado) {
        estadoItc.html(resultado.Estado);
        estadoItc.attr("title", resultado.Mensaje);
        var parentBadge = estadoItc.parent();
        if (resultado.Conectado) {
            //estadoItc.addClass("badge-success");
            //estadoItc.removeClass("badge-danger");
            parentBadge.css({
                'background-color': 'var(--green-100)',
                'color': 'var(--green-700)',
                'border-color': 'var(--green-200)'
            });
        } else {
            //estadoItc.addClass("badge-danger");
            //estadoItc.removeClass("badge-success");
            parentBadge.css({
                'background-color': '#fee2e2',
                'color': '#991b1b',
                'border-color': '#fecaca'
            });
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
        var bulb = $('#' + lectura.CodigoDispositivo);
        bulb.removeClass("bulb-unknown");
        if (lectura.Valor) {
            bulb.removeClass("bulb-inactive").addClass("bulb-active");
        } else {
            bulb.removeClass("bulb-active").addClass("bulb-inactive");
        }

        // No reset to inactive - maintain actual state
    };

    notificador.client.actualizarLecturaQr = function (lectura) {
        $('#' + lectura.CodigoDispositivo).val(lectura.QR);

        setTimeout(function () {
            $('#' + lectura.CodigoDispositivo).val("");
        }, 5000);
    };

    notificador.client.actualizarLecturaVehiculo = function (lectura) {
        var inputField = $('#' + lectura.CodigoDispositivo);
        var statusBadge = $('#status-' + lectura.CodigoDispositivo);
        
        inputField.val(lectura.Patente);
        statusBadge.removeClass("bulb-unknown");
        
        if (lectura.HayError) {
            inputField.css('color', 'red');
            statusBadge.removeClass("bulb-inactive").addClass("bulb-active");
        } else {
            inputField.css('color', '');
            statusBadge.removeClass("bulb-inactive").addClass("bulb-active");
        }
        
        setTimeout(function () {
            inputField.val("");
            inputField.css('color', '');
            statusBadge.removeClass("bulb-active").addClass("bulb-inactive");
        }, 5000);
    };

    notificador.client.actualizarEstadoDispositivo = function (estado) {
        if (!estado.Error) {
            MostrarAlertaExitosa("Conexión restablecida: " + estado.CodigoDispositivo);
        } else {
            MostrarAlertaError("Error en " + estado.CodigoDispositivo + ": " + estado.Mensaje);
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