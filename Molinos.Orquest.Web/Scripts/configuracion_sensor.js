$(document).ready(
    DriverSeleccionado
)
    
document.getElementById("ClaseDriver").addEventListener("change", DriverSeleccionado)

function DriverSeleccionado() {

    var driver = $("#ClaseDriver").val();

    if (driver.includes("ALPR") ) {
        $("#camaras").show();
        $("#acciones").hide();
        $("#Accion").val(null);
        return
    }
    else if(driver.includes("General") ){
        $("#camaras").hide();
        $("#acciones").show();
        $("#CamaraId").val(null);
        return
    }
 
    Cleaner();
}

function Cleaner(){
    $("#camaras").hide();
    $("#acciones").hide();
    $("#Accion").val(null);
    $("#CamaraId").val(null);
}

