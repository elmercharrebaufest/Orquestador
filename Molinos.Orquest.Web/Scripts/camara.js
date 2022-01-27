jQuery(function ($) {

    $('#imagen').attr("src", $('#Uri').val());

    if ($('#MargenDerecho').val() > 0) {
        $('#MargenDerecho').val($('#MargenDerecho').val());
    } else $('#MargenDerecho').val("0");

    if ($('#MargenIzquierdo').val() > 0) {
        $('#MargenIzquierdo').val($('#MargenIzquierdo').val());
    } else $('#MargenIzquierdo').val("0");

    if ($('#MargenSuperior').val() > 0) {
        $('#MargenSuperior').val($('#MargenSuperior').val());
    } else $('#MargenSuperior').val("0");

    if ($('#MargenInferior').val() > 0) {
        $('#MargenInferior').val($('#MargenInferior').val());
    } else $('#MargenInferior').val("0");

    setTimeout(function () {
        var ancho = $('#imagen')[0].width;
        var alto = $('#imagen')[0].height;        
        $("#rowImg").removeClass("hidden");
        $('#imagen').Jcrop({
            onChange: showCoords,
            onSelect: showCoords,
            bgColor: 'negro',
            bgOpacidad: .4,
            boxWidth: 450,
            aspectRadio: 16 / 9,
            boxHeigth: 400,
            setSelect: [parseInt($('#MargenIzquierdo').val()), parseInt($('#MargenSuperior').val()), ((parseInt($('#MargenDerecho').val()) - ancho) * -1), ((parseInt($('#MargenInferior').val()) - alto) * -1)]
        });
    }, 250); 

});

function showCoords(c) {

    var ancho = $('#imagen')[0].width;
    var alto = $('#imagen')[0].height;
    $('#MargenDerecho').val(Math.round(ancho - c.x2));
    $('#MargenIzquierdo').val(Math.round(c.x));
    $('#MargenSuperior').val(Math.round(c.y));
    $('#MargenInferior').val(Math.round(alto - c.y2));
    // variables can be accessed here as
    // c.x, c.y, c.x2, c.y2, c.w, c.h
};