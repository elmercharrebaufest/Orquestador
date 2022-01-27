$(document).ready(function () {

    var data = [
        {
            "type": "sunburst",
            "labels": [],
            "parents": [],
            "ids": [],
            "leaf": { "opacity": 0.4 },
            "marker": {
                "line": {
                    "width": 2
                },
                "colors": []
            },
            "branchvalues": 'total'
        }];
   
    var layout = {
        "margin": { "l": 0, "r": 0, "b": 0, "t": 0 },
        autosize: false,
        width: 1300,
        height: 580,
        extendunburstcolors: false,
        automargin: true
    };

    $.getJSON($('#dispositivos').data().urlBarrera, function (lista) {
        var fecha = lista.FechaUltimoEstado;
        data[0].labels = lista.Labels;
        data[0].parents = lista.Padres;
        data[0].ids = lista.Codigos;
        var activos = lista.Activos;
        var estadoCorrecto = lista.EstadoCorrecto;
        var colores = [];
        $("#dispositivos").html("<strong>Última actualización: </strong>" + new Date(parseInt(fecha.substr(6))));
        colores = ["#cccccc"];
        for (var i = 0; i < 10; i++) {
            if (data[0].labels[i] === "CabezalIPPorDemanda") {
                data[0].labels[i] = "Cabezal<br>Por<br>Demanda";
            } else if (data[0].labels[i] === "CabezalIPContinuo") {
                data[0].labels[i] = "Cabezal<br>Continuo";
            } else if (data[0].labels[i] === "CabezalIPDummy") {
                data[0].labels[i] = "Cabezal<br>Dummy";
            } else if (data[0].labels[i] === "HumedimetroContinuo") {
                data[0].labels[i] = "Humedimetro<br>Continuo";
            } 

        }
        for (i = 1; i < lista.Labels.length; i++) {
            
            data[0].labels[i] = data[0].labels[i];

            if (activos[i]) {
                if (estadoCorrecto[(i)]) {
                    colores.push("#009432");
                } else {
                    colores.push("#ff3333");
                }
            } else {
                colores.push("#cccccc");
            }
        }

        data[0].marker.colors = colores;
        Plotly.newPlot('myDiv', data, layout, { displaylogo: false });
    });

    $('#myDiv').on("plotly_click", function (a, b) {
        if (b.points[0].id == "Dispositivos") {
            return true;
        }
        if (b.points[0].parent == "CabezalIPPorDemanda" || b.points[0].parent == "CabezalIPContinuo" || b.points[0].parent == "CabezalIPDummy") {
            b.points[0].parent = "Cabezal";
        } if (b.points[0].parent == "HumedimetroContinuo"){
            b.points[0].parent = "Humedimetro";
        }
        window.location = $("#urlPrueba").data().url + "?codigoDispositivo=" + b.points[0].id + "&path=" + b.points[0].parent.split("<br>")[0]
    })
});

