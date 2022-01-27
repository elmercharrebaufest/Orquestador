/* Se sobreescriben las valildaciones de numeros y fechas para que usen Globalize*/



function EscaparPunto(value) {
    return value == "." ? "\\." : value;
}

$.validator.methods.number = function (value, element) {
    //FP: Revisar la propiedad numberFormat de la globalize 0.1 que no existe mas
    var groupSep = EscaparPunto(Globalize.cldr.main("numbers/symbols-numberSystem-" + Globalize.cldr.main("numbers/defaultNumberingSystem") + "/group"));
    var decimalSep = EscaparPunto(Globalize.cldr.main("numbers/symbols-numberSystem-" + Globalize.cldr.main("numbers/defaultNumberingSystem") + "/decimal"));
    var exp = new RegExp("^-?\(?:\\d+|\\d{1,3}\(?:" + groupSep + "\\d{3}\)+\)?\(?:" + decimalSep + "\\d+\)?$");
    return this.optional(element) || exp.test(value);
};

$.validator.methods.date = function (value, element) {
    return this.optional(element) || Globalize.parseDate(value) != null;
};

$.validator.methods.range = function (value, element, param) {

    var val = Globalize.parseNumber(value);

    return this.optional(element) || (val >= param[0] && val <= param[1]);
};

/* Localización del date picker*/
$(function () {

    if ($.datepicker.regional[Globalize.locale().locale]) {
        /* Si UI tiene la cultura, usarla*/
        $.datepicker.setDefaults($.datepicker.regional[Globalize.locale().locale]);
    } else if ($.datepicker.regional[Globalize.locale().attributes["language"]]) {
        /* Si no tiene la cultura pero tiene el lenguaje usarlo   */
        $.datepicker.setDefaults($.datepicker.regional[Globalize.locale().attributes["language"]]);
    } else {
        /*Si no tiene ninguno de los dos dejamos el valor por defecto (Ingles)*/
        $.datepicker.setDefaults($.datepicker.regional[""]);
    }
});