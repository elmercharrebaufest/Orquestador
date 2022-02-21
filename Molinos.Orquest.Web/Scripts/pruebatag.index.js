$("#boton-tag").click(function () {
    var self = $("#boton-tag");
    var div = $("#resultado-peso");
    div.html(div.data().textoCarga);

    $.getJSON(self.data().urlActivacion, function (resultado) {
          
        if (resultado.Codigo == 0) {
            div.html(resultado.Mensaje);
        } else {
            div.html(resultado.Mensaje);
        }
    });
});