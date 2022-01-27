$(function () {
    $("#link-ping").click(function () {    
        var div = $("#resultado-ping");
        div.html(div.data().textoCarga);
        $.get(this.href, function (resultado) {
            div.html(resultado);
        });
        return false;
    });
   
    
    $("#link-loop").click(function () {
        var div = $("#resultado-loop");
        div.html(div.data().textoCarga);
        var repeticiones = parseInt($("#repeticiones").val());
        $.get(this.href, {repeticiones: repeticiones}, function (resultado) {
            div.html(resultado);
        });
        return false;
    });
    
    $('.numerico').keydown(function (event) {
        return (event.keyCode >= 48 && event.keyCode <= 57) || (event.keyCode >= 96 && event.keyCode <= 105) ||
                    event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 16 ||
                    event.keyCode == 39 || event.keyCode == 37 || event.keyCode == 46 ||
                    event.keyCode == 13;
    });
});