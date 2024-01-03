function inicializarFormularioTransacciones(urlObtenerCategorias) {
    $("#TipoOperacionId").change(async function () {
        const valorSeleccionado = $(this).val();

        const respuesta = await fetch(urlObtenerCategorias, {
            method: 'POST',
            body: valorSeleccionado,
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const json = await respuesta.json();
        //console.log(json); // Muestra en consola las categorias por tipo de operacion
        const opciones = json.map(categoria => `<option value=${categoria.value}>${categoria.text}</option>`);
        $("#CategoriaId").html(opciones); // Genera un arreglo de opciones y las inserta en el html de CategoriaId
    })
}