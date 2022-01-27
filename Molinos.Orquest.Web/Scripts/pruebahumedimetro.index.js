$(function () {
	$("#link-humedad").click(function () {
		var div = $("#resultado-humedad");
		div.html(div.data().textoCarga);
		$.get(this.href, function (resultado) {
			div.html(resultado);
		});
		return false;
	});
});