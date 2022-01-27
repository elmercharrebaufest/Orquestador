$(function () {
	$("#link-peso").click(function () {
		var div = $("#resultado-peso");
		div.html(div.data().textoCarga);
		$.get(this.href, function (resultado) {
			div.html(resultado);
		});
		return false;
	});
});