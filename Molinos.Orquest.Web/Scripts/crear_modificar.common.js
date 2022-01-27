
$(document).ready(function () {
    if ($("#Dispositivo_EsConcentrador").is(':checked')) {
        $("#Dispositivo_ConcentradorId").val("0");
        $("#Dispositivo_ConcentradorId").attr('disabled',true);
    }
    $("#Dispositivo_EsConcentrador").click(function () {
        $("#Dispositivo_ConcentradorId").attr('disabled', this.checked);
        if (this.checked)
            $("#Dispositivo_ConcentradorId").val("0");
    });
});
