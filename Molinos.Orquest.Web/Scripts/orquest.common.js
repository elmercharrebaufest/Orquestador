var deleteLinkObj;
var confirmarLinkObj;

$(document).ready(function () {
    if (!$("#alerta").hasClass("hide")) {
        $("#alerta").delay(500).addClass("in");
    }

    attachDataPickers();

    $(document).on('click', '#grid thead th a, #grid tfoot td a', function (evt) {
        var container = $(this).parents('#gridContainer');
        if (container.attr('data-grid-url')) {
            container.data().gridUrl = this.href;
        }
        $.get(this.href, function (data) {
            container.html(data);
        });
        return false;
    });

    /* delete Link */
    $(document).on('click', '.ajax-borrar-link', function () {
        deleteLinkObj = $(this); /*for future use*/
        $('#dialogo-borrar').modal({
            keyboard: false
        });
        return false; /* prevents the default behaviour */
    });

    $('#dialogo-borrar').on('shown', function () {
        $('#dialogo-borrar-cancelar').focus();
    });

    $('#dialogo-borrar-cancelar').click(function () {
        $('#dialogo-borrar').modal('hide');
    });

    $('#dialogo-borrar-confirmar').click(function () {
        $.post(deleteLinkObj[0].href, function (data) { /*Post to action*/
            if (data == 'true') {
                deleteLinkObj.closest("tr").hide(); /*Hide Row*/
                MostrarAlertaExitosa();
            } else {
                MostrarAlertaError(data);
            }
        }).fail(
            function (message) {
                MostrarAlertaError(message);
            });
        $('#dialogo-borrar').modal('hide');
    });

    //Inicio Modal Editar : Script encargado de manejar el contendido y comportamiento
    //del modal.
    $(document).on('click', '.ajax-editar-link', function () {
        $.get(this.href, cargarDialogoEditar);
        return false;
    });

    $('#dialogo-editar-cancelar').click(function () {
        $('#dialogo-editar').modal('hide');
    });

    $('#dialogo-editar-guardar').click(function () {
        $('#dialogo-editar form').submit();
        if ($('#dialogo-editar form').valid())
            $("#dialogo-editar-guardar").attr("disabled", true);
    });

    $('#dialogo-editar').on('show', function () {
        $(this).find('#modal-editar-body').css({
            height: 'auto', 'max-height': '400px', 'padding-right': '50px'
        });
    });

    $('#dialogo-editar').on('shown', function () {
        $(this).find('#modal-editar-body').find(':input:enabled:visible:first').focus();
    });
    //Fin Modal Editar

    $('#dialogo-ver-ok').click(function () {
        $('#dialogo-ver').modal('hide');
    });

    $(document).on('click', '.ajax-ver-link', function () {
        $.get(this.href, cargarDialogoVer);
        return false;
    });

    /* Generic Ajax error handling */
    $("#error-box").ajaxError(function (event, jqXhr, ajaxSettings, thrownError) {
        var $this = $(this);
        var errorText = $this.data().genericError + jqXhr.responseText;
        $this.html(errorText);
        $('#error-box-container').slideDown('slow');
    });

    $(document).on('keypress', '.to-uppercase', forceUppercase)
});

function forceUppercase(e) {
    var charInput = e.keyCode;
    if ((charInput >= 97) && (charInput <= 122)) { // lowercase - no incluye ñ
        if (!e.ctrlKey && !e.metaKey && !e.altKey) { // no modifier key
            var newChar = charInput - 32;
            var start = e.target.selectionStart;
            var end = e.target.selectionEnd;
            e.target.value = e.target.value.substring(0, start) + String.fromCharCode(newChar) + e.target.value.substring(end);
            e.target.setSelectionRange(start + 1, start + 1);
            e.preventDefault();
        }
    }
}


$(document).ready(function () {
    /*Centrar la primera vez*/
    CentrarPosicionElemento();
    /*Centrar por redimensión de pantalla*/
    $(window).resize(function (e) { e.preventDefault(); CentrarPosicionElemento(); });
});

function cargarDialogoEditar(data) {
    $('#modal-editar-body').html(data);
    $("#dialogo-editar-guardar").attr("disabled", false);
    $('#dialogo-editar-title').html($('#modal-editar-body form').data().dialogoTitulo);
    $('#modal-editar-body form').attr('data-ajax-success', 'editarRepuestaFormulario');
    if ($('#modal-editar-body form').data().dialogoExtraclass) {
        $('#dialogo-editar').addClass($('#modal-editar-body form').data().dialogoExtraclass);
    }

    $('#dialogo-editar').modal({
        backdrop: 'static', keyboard: false
    }).css({
            'top': '30%',
            'margin-left': function () {                 return -($(this).width() / 2);
            },
            'left': '50%',
        'margin-top': function () {
            return -160;
        }
    });

    attachDataPickers();
}

function cargarDialogoVer(data) {
    $('#modal-ver-body').html(data);
    $('#modal-ver-body form').attr('data-ajax-success', 'editarRepuestaFormulario');
    $('#dialogo-ver').modal({
        backdrop: 'static', keyboard: false
    });
}

function attachDataPickers() {
    $('input.date').each(function () {
        var $this = $(this);
        $this.datepicker();
    });
}

function editarRepuestaFormulario(respuesta) {
    if (respuesta == window.ajaxEditSuccess) {
        window.location.href = window.location.href;
    } else {
        cargarDialogoEditar(respuesta);
    }
}

function editarRepuestaFormulario(respuesta) {
    if (respuesta == window.ajaxEditSuccess) {
        if ($('#search-form').length == 0) {
            window.location.href = window.location.href;
        } else {
            $('#dialogo-editar').modal('hide');
            $('#search-form').submit();
            MostrarAlertaExitosa();
        }
    } else {
        cargarDialogoEditar(respuesta);
    }
}

function MostrarAlertaError(data) {
    if (data != null) {
        $("#alertaError-body h4").text(data);
    } else {
        $("#alertaError-body h4").text($("#alertaError").data().mensaje);
    }
    $("#alertaError").modal('show', {
        keyboard: false
    });
}

function MostrarAlertaAdvertencia(data) {
    if (data != null) {
        $("#alertaAdvertencia-body h4").text(data);
    } else {
        $("#alertaAdvertencia-body h4").text($("#alertaAdvertencia").data().mensaje);
    }
    $("#alertaAdvertencia").modal('show', {
        keyboard: false
    });
}

function MostrarAlertaExitosa(data) {
    if (data != null) {
        $("#alertaExitosa-body h4").text(data);
    }
    $("#alertaExitosa").modal('show', {
        keyboard: false
    });
}

function MostrarAlertaCancelada() {
    $("#alertaCancelada").modal("show", {
        keyboard: false
    });
}

function CentrarPosicionElemento() {
    $('.centro-pantalla').css({
        position: 'fixed',
        left: ($(window).width() - $('.centro-pantalla').outerWidth()) / 2,
        top: ($(window).height() - $('.centro-pantalla').outerHeight()) / 3
    });
}

function BlockUI(message) {
    message = message != undefined ? message : '';
    message = $("#Procesando").val() + " " + message + ", " + $("#PorFavorEspere").val();
    $('#loading').text(message);
    var loading = $('#loading');
    var height = $(window).height();
    var width = $(document).width();
    $.blockUI.defaults.css = {
        left: width / 2 - (loading.width() / 2),
        top: height / 3 - (loading.height() / 3),
        backgroundColor: 'white',
        border: '1px solid #B94A41',
        color: '#0055A5',
        "padding-right": 20,
        paddingTop: 10,
        paddingBottom: 10,
        "padding-left": 20,
    };
    $.blockUI({
        overlayCSS: { backgroundColor: 'white' },
        message: $('#loading').html(),
        baseZ: 2000
    });
}


