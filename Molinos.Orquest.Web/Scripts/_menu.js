var esElOrigenDeLaNotificacion = false;
var estaActualizado = false;
$(document).ready(function () {

    $('#idiomas li').on('click', (function () {
        $('#idiomaBoton').text($(this).data().idioma);
    }));

});

function actualizarTimeAgo(server, hora) {
    var serverd = new Date(server);
    var horad = new Date(hora);
    var distance = (serverd.getTime() - horad.getTime()),
				seconds = Math.abs(distance) / 1000,
				minutes = seconds / 60,
				hours = minutes / 60;
    if (hours < 24) {
        return Globalize.format(horad, Globalize.culture().calendars.standard.patterns.t);
    } else {
        return Globalize.format(horad, 'D');
    }
}

(function ($) {
    $.whenAll = function (deferreds) {
        return $.when.apply($, deferreds);
    };
})(jQuery);




