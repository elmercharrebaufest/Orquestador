function Permiso(id, descripcion) {
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
    self.permisos = ko.observableArray([]);
    self.newPermisoId = ko.observable();
    self.newPermisoDescripcion = ko.observable();

    $.get("Rol/ObtenerPermisos", { id: $('#Id').val() },
        function (allData) {
            var mappedPermisos = $.map(allData, function (item) {
                //inhabilito la opcion
                $("#permiso option[value=" + item.Id + "]").attr('disabled', 'disabled');
                setearDropDownSelected();
                return new Permiso(item);
            });
            self.permisos(mappedPermisos);
            $('#permisosFinales').val(ko.toJSON(self.permisos));
        }
    );

    $("#permiso option:selected").removeAttr("selected");

    // Operations
    self.addPermiso = function () {
        if ($('#permiso option:selected').text() > "") {
            self.permisos.push(new Permiso($('#permiso option:selected').val(), $('#permiso option:selected').text()));
            //inhabilito la opcion
            $("#permiso option:selected").attr('disabled', 'disabled');
            setearDropDownSelected();
            self.newPermisoDescripcion("");
        }
        $('#permisosFinales').val(ko.toJSON(self.permisos));

    };

    self.removePermiso = function (permiso) {
        //habilito la opcion
        $("#permiso option[value=" + permiso.Id + "]").removeAttr('disabled');
        self.permisos.remove(permiso);
        $('#permisosFinales').val(ko.toJSON(self.permisos));
    };

}

function setearDropDownSelected() {
    var seleccionado = false;
    $.each($("#permiso option"), function (index, value) {
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
    ko.applyBindings(new RolListViewModel(), document.getElementById('permisosViewModel'));
});
