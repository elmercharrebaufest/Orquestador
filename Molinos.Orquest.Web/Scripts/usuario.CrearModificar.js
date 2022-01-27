function Rol(id, descripcion) {
    if ($.isNumeric(parseInt(id))) {
        this.Id = id;
        this.Descripcion = descripcion;
    } else {
        //id trae el objeto que ya existia
        this.Id = id.Id;
        this.Descripcion = id.Descripcion;
    }
}

function UsuarioListViewModel() {
    var self = this;
    self.roles = ko.observableArray([]);

    $.getJSON($("#links").data().urlObtenerRoles, { id: $('#Id').val() },
        function (allData) {
            var mappedRoles = $.map(allData, function (item) {
                //inhabilito la opcion
                $("#rol option[value=" + item.Id + "]").attr('disabled', 'disabled');
                setearDropDownRolSelected();
                return new Rol(item);
            });
            self.roles(mappedRoles);
            $('#rolesFinales').val(ko.toJSON(mappedRoles));
        }
    );

    // Operations
    self.addRol = function () {
        if ($('#rol option:selected').text() > "") {
            self.roles.push(new Rol($('#rol option:selected').val(), $('#rol option:selected').text()));
            //inhabilito la opcion
            $("#rol option:selected").attr('disabled', 'disabled');
            setearDropDownRolSelected();
        }
        $('#rolesFinales').val(ko.toJSON(self.roles));

    };
    self.removeRol = function (rol) {
        //habilito la opcion
        $("#rol option[value=" + rol.Id + "]").removeAttr('disabled');
        self.roles.remove(rol);
        $('#rolesFinales').val(ko.toJSON(self.roles));
    };
}

function setearDropDownRolSelected() {
    var seleccionado = false;
    $.each($("#rol option"), function (index, value) {
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

    ko.applyBindings(new UsuarioListViewModel());
});
