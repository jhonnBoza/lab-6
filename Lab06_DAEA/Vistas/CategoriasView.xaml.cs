using System.Windows;
using System.Windows.Controls;
using Lab06_DAEA.Datos;
using Lab06_DAEA.Datos.Modelos;

namespace Lab06_DAEA.Vistas;

public partial class CategoriasView : UserControl
{
    private readonly CategoriaRepositorio _repositorio = new();

    private int? _idEnEdicion;
    private bool _activaEnEdicion = true;

    public CategoriasView() => InitializeComponent();

    private async void AlCargar(object sender, RoutedEventArgs e)
    {
        await CargarCategoriasAsync();
        LimpiarFormulario();
    }

    private async Task CargarCategoriasAsync()
    {
        try
        {
            var incluirInactivos = MostrarInactivos.IsChecked == true;
            var categorias = await _repositorio.ListarAsync(incluirInactivos);
            GrillaCategorias.ItemsSource = categorias;

            var dadasDeBaja = categorias.Count(c => !c.Activo);

            TextoEstado.Text = incluirInactivos
                ? $"{categorias.Count} categoria(s), de las cuales {dadasDeBaja} estan dadas de baja."
                : $"{categorias.Count} categoria(s) activas.";
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo cargar el listado de categorias.", ex);
        }
    }

    private async void AlActualizarLista(object sender, RoutedEventArgs e)
    {
        await CargarCategoriasAsync();
        LimpiarFormulario();
    }

    private async void AlCambiarVisibilidad(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        await CargarCategoriasAsync();
        LimpiarFormulario();
    }

    private void AlSeleccionarCategoria(object sender, SelectionChangedEventArgs e)
    {
        if (GrillaCategorias.SelectedItem is not Categoria categoria)
        {
            return;
        }

        _idEnEdicion          = categoria.IdCategoria;
        _activaEnEdicion      = categoria.Activo;
        CampoId.Text          = categoria.IdCategoria.ToString();
        CampoNombre.Text      = categoria.NombreCategoria;
        CampoDescripcion.Text = categoria.Descripcion ?? string.Empty;

        TextoModo.Text = categoria.Activo
            ? $"CATEGORIA #{categoria.IdCategoria}"
            : $"CATEGORIA #{categoria.IdCategoria} (DADA DE BAJA)";

        BotonEliminar.IsEnabled  = categoria.Activo;
        BotonRestaurar.IsEnabled = !categoria.Activo;
    }

    private void AlNuevo(object sender, RoutedEventArgs e)
    {
        GrillaCategorias.SelectedItem = null;
        LimpiarFormulario();
        CampoNombre.Focus();
    }

    private async void AlGuardar(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CampoNombre.Text))
        {
            Mensajes.Advertencia("El nombre de la categoria es obligatorio.");
            CampoNombre.Focus();
            return;
        }

        var categoria = new Categoria
        {
            IdCategoria     = _idEnEdicion ?? 0,
            NombreCategoria = CampoNombre.Text.Trim(),
            Descripcion     = string.IsNullOrWhiteSpace(CampoDescripcion.Text)
                                  ? null
                                  : CampoDescripcion.Text.Trim()
        };

        try
        {
            if (_idEnEdicion is null)
            {
                var nuevoId = await _repositorio.InsertarAsync(categoria);
                TextoEstado.Text = $"Categoria registrada con el ID {nuevoId}.";
            }
            else
            {
                await _repositorio.ActualizarAsync(categoria);
                TextoEstado.Text = $"Categoria #{categoria.IdCategoria} actualizada.";
            }

            await CargarCategoriasAsync();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo guardar la categoria.", ex);
        }
    }

    private async void AlEliminar(object sender, RoutedEventArgs e)
    {
        if (_idEnEdicion is null || !_activaEnEdicion)
        {
            return;
        }

        if (!Mensajes.Confirmar(
                $"Se dara de baja la categoria #{_idEnEdicion}.\n\n" +
                "La fila no se borra: queda con Activo = 0 y deja de aparecer en el " +
                "listado. Podra restaurarla despues. Desea continuar?"))
        {
            return;
        }

        try
        {
            var filas = await _repositorio.EliminarAsync(_idEnEdicion.Value);
            TextoEstado.Text = $"Categoria #{_idEnEdicion} dada de baja ({filas} fila actualizada).";

            await CargarCategoriasAsync();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo dar de baja la categoria.", ex);
        }
    }

    private async void AlRestaurar(object sender, RoutedEventArgs e)
    {
        if (_idEnEdicion is null || _activaEnEdicion)
        {
            return;
        }

        try
        {
            var filas = await _repositorio.RestaurarAsync(_idEnEdicion.Value);
            TextoEstado.Text = $"Categoria #{_idEnEdicion} restaurada ({filas} fila actualizada).";

            await CargarCategoriasAsync();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo restaurar la categoria.", ex);
        }
    }

    private void LimpiarFormulario()
    {
        _idEnEdicion             = null;
        _activaEnEdicion         = true;
        CampoId.Text             = "(automatico)";
        CampoNombre.Text         = string.Empty;
        CampoDescripcion.Text    = string.Empty;
        TextoModo.Text           = "NUEVA CATEGORIA";
        BotonEliminar.IsEnabled  = false;
        BotonRestaurar.IsEnabled = false;
    }
}
