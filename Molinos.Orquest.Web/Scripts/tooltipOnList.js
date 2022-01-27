$(document).ready(function () {
    $(".mostrar-lista").hover(function () {
        var tip = $(this).find('.tip');
        tip.show(); //Show tooltip 
    }, function () {
        var tip = $(this).find('.tip');
        tip.hide(); //Hide tooltip 

    }).mousemove(function (e) {
        var tip = $(this).find('.tip');
        var mousex = e.pageX; //Get X coodrinates 
        var mousey = e.pageY; //Get Y coordinates 
        var tipWidth = tip.width(); //Find width of tooltip 
        var tipHeight = tip.height(); //Find height of tooltip 
        //Distance of element from the right edge of viewport 
        var tipVisX = $(window).width() - (mousex + tipWidth);
        //Distance of element from the bottom of viewport 
        var tipVisY = $(window).height() - (mousey + tipHeight);
        if (tipVisX < 20) { //If tooltip exceeds the X coordinate of viewport 
            mousex = e.pageX - tipWidth - 20;
        }
        if (tipVisY < 20) { //If tooltip exceeds the Y coordinate of viewport 
            mousey = e.pageY - tipHeight - 20;
        }
        //Absolute position the tooltip according to mouse position 
        tip.css({ top: mousey, left: mousex });
    });
});