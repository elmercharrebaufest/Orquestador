$(function (){
	$(".estado-itc").each(function () {
        var $this = $(this);
        $.getJSON($("#gridContainer").data().urlEstado, {codigoDispositivo: $this.data().codigo}, function (resultado) {
            $this.html(resultado.Estado);
            $this.attr("title", resultado.Mensaje);
            if (resultado.Conectado) {
                $this.addClass("badge-success");
            } else {
                $this.addClass("badge-danger");
            }
        });
    });
});