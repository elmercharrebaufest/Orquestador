$(document).ready(function () {

    jQuery(document).on('click', '.detener', function () {
        BlockUI();
        $.ajax({
            url: $(this).attr('href'),
            dataType: "json",
            type: "GET",
            error: function () {
                console.log("Ocurrio un error detener el servicio");
                MostrarAlertaError("Ocurrio un error detener el servicio");
            },
            success: function () {
                MostrarAlertaExitosa();
            }
        }).always(function () {
            //cargarEstado();
            $.unblockUI();
        });
        return false;
    });

    jQuery(document).on('click', '.iniciar', function () {
        BlockUI();
        $.ajax({
            url: $(this).attr('href'),
            dataType: "json",
            type: "GET",
            error: function () {
                console.log("Ocurrio un error iniciar el servicio");
                MostrarAlertaError("Ocurrio un error iniciar el servicio");
            },
            success: function () {
                MostrarAlertaExitosa("El servicio fue iniciado");
            }
        }).always(function () {
            //cargarEstado();
            $.unblockUI();
        });
        return false;
    });
    
    //cargarEstado();
});

function cargarEstado() {
    var s = $('#cantServers').val();
    for (var i = 0; i < s; i++) {
        (function(index) {
            $.ajax({
                url: $('.estadoServ' + '.' + index).text(),
                dataType: "json",
                async: false,
                type: "GET",
                error: function(data) {
                    console.log("Ocurrio un error al obtener estado" + data);
                },
                success: function(data) {
                    console.log(data);
                    $("[id='estadoTxt " + index + "'" + "]")[0].innerHTML = data;
                }
            });
        })(i);
    }
}
