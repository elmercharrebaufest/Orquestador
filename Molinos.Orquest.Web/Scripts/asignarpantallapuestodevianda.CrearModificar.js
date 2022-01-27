function Pantalla(id, descripcion) {
    if ($.isNumeric(parseInt(id))) {
        this.Id = id;
        this.Descripcion = descripcion;
    } else {
        //id trae el objeto que ya existia
        this.Id = id.Id;
        this.Descripcion = id.Descripcion;
    }
}

function RolListViewModel() {
    var self = this;
    self.pantallas = ko.observableArray([]);
    self.newPantallaId = ko.observable();
    self.newPantallaDescripcion = ko.observable();

    $.get("AsignarPantallaPuestoDeVianda/ObtenerPantallas", { id: $('#Id').val() },
        function (allData) {
            var mappedPantallas = $.map(allData, function (item) {
                //inhabilito la opcion
                $("#pantalla option[value=" + item.Id + "]").attr('disabled', 'disabled');
                setearDropDownSelected();
                return new Pantalla(item);
            });
            self.pantallas(mappedPantallas);
            $('#pantallasFinales').val(ko.toJSON(self.pantallas));
        }
    );

    $("#pantalla option:selected").removeAttr("selected");

    // Operations
    self.addPantalla = function () {
        if ($('#pantalla option:selected').text() > "") {
            self.pantallas.push(new Pantalla($('#pantalla option:selected').val(), $('#pantalla option:selected').text()));
            //inhabilito la opcion
            $("#pantalla option:selected").attr('disabled', 'disabled');
            setearDropDownSelected();
            self.newPantallaDescripcion("");
        }
        $('#pantallasFinales').val(ko.toJSON(self.pantallas));

    };

    self.removePantalla = function (pantalla) {
        //habilito la opcion
        $("#pantalla option[value=" + pantalla.Id + "]").removeAttr('disabled');
        self.pantallas.remove(pantalla);
        $('#pantallasFinales').val(ko.toJSON(self.pantallas));
    };

}

function setearDropDownSelected() {
    var seleccionado = false;
    $.each($("#pantalla option"), function (index, value) {
        if (value.disabled) {
            value.selected = false;
        } else {
            if (!seleccionado) {
                value.selected = true;
                seleccionado = true;
            }
        }
    });
}

$(document).ready(function () {
    ko.applyBindings(new RolListViewModel(), document.getElementById('pantallasViewModel'));
});
