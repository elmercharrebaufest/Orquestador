$(function () {
    $("#link-consulta").click(function () {
        event.preventDefault();
        var div = $("#resultado-consulta");
        if (validarValorConsultaBalanzada()) {
            var url = $("#link-consulta").attr("href") + "&idBalanzada=" + $('#idBalanzada').val();

            div.html(div.data().textoCarga);
            $.get(url, function (resultado) {
                div.html(resultado);
            });
            return false;
        } else {
            div.html("El valor ingresado no es válido. Debe ser un número positivo.");
        }
    });

    $("#link-borrado").click(function () {
        event.preventDefault();
        var div = $("#resultado-borrado");

        if (validarValorBorradoBalanzada()) {
            var url = $("#link-borrado").attr("href") + "&idBalanzadaBorrado=" + $('#idBalanzadaBorrado').val();
            div.html(div.data().textoCarga);
            $.get(url, function (resultado) {
                div.html(resultado);
            });
            return false;
        } else {
            div.html("El valor ingresado no es válido. Debe ser un número positivo.");
        }
    });

    $("#link-borrado-por-rango").click(function () {
        event.preventDefault();
        var div = $("#resultado-borrado-por-rango");
        if (validarValoresBorradoBalanzadaPorRango()) {
            var url = $("#link-borrado-por-rango").attr("href") + "&idBalanzadaBorradoInicio=" + $('#idBalanzadaBorradoInicio').val() + "&idBalanzadaBorradoFin=" + $('#idBalanzadaBorradoFin').val();

            div.html(div.data().textoCarga);
            $.get(url, function (resultado) {
                div.html(resultado);
            });
            return false;

        } else {
            div.html('Error en los valores ingresados. Deben ser números positivos y el inicio menor o igual al fin.');
        }
    });

    $("#link-consulta-por-rango").click(function () {
        event.preventDefault();
        var div = $("#resultado-consulta-por-rango");
        if (validarValoresConsultaBalanzadaPorRango()) {

            var url = $("#link-consulta-por-rango").attr("href") + "&idBalanzadaInicio=" + $('#idBalanzadaInicio').val() + "&idBalanzadaFin=" + $('#idBalanzadaFin').val();

            div.html(div.data().textoCarga);
            $.get(url, function (resultado) {
                div.html(resultado);
            });
            return false;
        } else {
            div.html('Error en los valores ingresados. Deben ser números positivos y el inicio menor o igual al fin.');
        }
    });

    function validarValorConsultaBalanzada() {
        if ($.isNumeric($("#idBalanzada").val())) {
            if ($("#idBalanzada").val() <= 0) {
                return false;
            } else {
                return true;
            }
        } else {
            if ($("#idBalanzada").val() !== '') {
                return false;
            } else {
                return true;
            }
        }
    }

    function validarValoresConsultaBalanzadaPorRango() {
        if ($.isNumeric($("#idBalanzadaInicio").val()) && $.isNumeric($("#idBalanzadaFin").val())) {
            if ($("#idBalanzadaInicio").val() <= 0 || $("#idBalanzadaFin").val() <= 0 || Number($("#idBalanzadaInicio").val()) >= Number($("#idBalanzadaFin").val())) {
                return false;
            } else {
                return true;
            }
        } else {
            return false;
        }
    }

    function validarValorBorradoBalanzada() {
        if ($.isNumeric($("#idBalanzadaBorrado").val())) {
            if ($("#idBalanzadaBorrado").val() <= 0) {
                return false;
            } else {
                return true;
            }
        }
    }

    function validarValoresBorradoBalanzadaPorRango() {
        if ($.isNumeric($("#idBalanzadaBorradoInicio").val()) && $.isNumeric($("#idBalanzadaBorradoFin").val())) {
            if ($("#idBalanzadaBorradoInicio").val() <= 0 || $("#idBalanzadaBorradoFin").val() <= 0 || Number($("#idBalanzadaBorradoInicio").val()) >= Number($("#idBalanzadaBorradoFin").val())) {
                return false;
            } else {
                return true;
            }
        } else {
            return false;
        }
    }

});