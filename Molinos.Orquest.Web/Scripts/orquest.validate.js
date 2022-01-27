$(document).ready(function () {
    /*Deshabilito el boton guardar del modal para no generar requests repetidos */
    if ($("#dialogo-editar-guardar").length > 1)
        $("#dialogo-editar-guardar").attr("disabled", true);

    /*Agrego clase de validación para Bootstrap*/
    $('span.field-validation-valid, span.field-validation-error').each(function () {
        $(this).addClass('help-block');
    });

    /*Agrego clase error si hay errores lado cliente*/
    $('form').submit(function () {
        $(this).find('div.form-group').each(function () {
            if ($(this).find('span.field-validation-error').length == 0) {
                $(this).removeClass('error');
            }
        });
        $(this).find('div.form-group').each(function () {
            if ($(this).find('span.field-validation-error').length > 0) {
                $(this).addClass('error');
            }
        });
    });

    /*Agrego clase error si hay errores lado servidor*/
    $(this).find('div.form-group').each(function () {
        if ($(this).find('span.field-validation-error').length == 0) {
            $(this).removeClass('error');
        }
    });
    $(this).find('div.form-group').each(function () {
        if ($(this).find('span.field-validation-error').length > 0) {
            $(this).addClass('error');
        }
    });

    /*Vierifico si los errores siguen persistiendo*/
    $('input, textarea, select').change(function () {
        var controlGroup = $(this).closest("div.form-group");
        if (controlGroup.find('span.field-validation-error').length == 0)
            controlGroup.removeClass('error');
        else
            controlGroup.addClass('error');
    });

    $('input, textarea').keyup(function () {
        var controlGroup = $(this).closest("div.form-group");
        if (controlGroup.find('span.field-validation-error').length == 0)
            controlGroup.removeClass('error');
        else
            controlGroup.addClass('error');
    });

    $('input, textarea, select').blur(function () {
        var controlGroup = $(this).closest("div.form-group");
        if (controlGroup.find('span.field-validation-error').length == 0)
            controlGroup.removeClass('error');
        else
            controlGroup.addClass('error');
    });

    $('form').keypress(function (e) {
        if ((e.keyCode == 13) && (e.target.type != "textarea")) {
            e.preventDefault();
            $(this).submit();
        }
    });

});

function ValidarObjeto(formulario, elemento) {
    formulario.validate().element(elemento);
    var controlGroup = elemento.closest("div.form-group");
    if (controlGroup.find('span.field-validation-error').length == 0)
        controlGroup.removeClass('error');
    else
        controlGroup.addClass('error');
}