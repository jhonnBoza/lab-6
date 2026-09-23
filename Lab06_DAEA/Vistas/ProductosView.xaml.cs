using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Lab06_DAEA.Datos;
using Lab06_DAEA.Datos.Modelos;

namespace Lab06_DAEA.Vistas;

public partial class ProductosView : UserControl
{
    private readonly ProductoRepositorio  _productos   = new();
    private readonly CategoriaRepositorio _categorias  = new();
    private readonly ProveedorRepositorio _proveedores = new();

    private List<Producto> _listado = [];
    private int? _idEnEdicion;
    private bool _activoEnEdicion = true;

    public ProductosView() => InitializeComponent();

    private async void AlCargar(object sender, RoutedEventArgs e)
    {
        await CargarCombosAsync();
        await CargarProductosAsync();
        LimpiarFormulario();
    }

    private async Task CargarCombosAsync()
    {
        try
        {
            ComboCategorias.ItemsSource  = await _categorias.ListarAsync();
            ComboProveedores.ItemsSource = await _proveedores.ListarAsync();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudieron cargar las categorias y proveedores.", ex);
        }
    }

    private async Task CargarProductosAsync()
    {
        try
        {
            _listado = await _productos.ListarAsync(MostrarInactivos.IsChecked == true);
            AplicarFiltro();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo cargar el listado de productos.", ex);
        }
    }

    private void AplicarFiltro()
    {
        var texto = FiltroTexto.Text.Trim();

        var visibles = string.IsNullOrEmpty(texto)
            ? _listado
            : _listado.Where(p =>
                  Contiene(p.NombreProducto, texto) ||
                  Contiene(p.NombreCategoria, texto) ||
                  Contiene(p.NombreProveedor, texto)).ToList();

        GrillaProductos.ItemsSource = visibles;

        var dadosDeBaja = visibles.Count(p => !p.Activo);

        var resumen = visibles.Count == _listado.Count
            ? $"{_listado.Count} producto(s) en el listado."
            : $"{visibles.Count} de {_listado.Count} producto(s) coinciden con '{texto}'.";

        TextoEstado.Text = dadosDeBaja == 0
            ? resumen
            : $"{resumen} {dadosDeBaja} estan dados de baja.";
    }

    private static bool Contiene(string? valor, string texto)
        => valor is not null && valor.Contains(texto, StringComparison.OrdinalIgnoreCase);

    private void AlCambiarFiltro(object sender, TextChangedEventArgs e)
    {
        if (IsLoaded)
        {
            AplicarFiltro();
        }
    }

    private async void AlActualizarLista(object sender, RoutedEventArgs e)
    {
        await CargarCombosAsync();
        await CargarProductosAsync();
        LimpiarFormulario();
    }

    private async void AlCambiarVisibilidad(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        await CargarProductosAsync();
        LimpiarFormulario();
    }

    private void AlSeleccionarProducto(object sender, SelectionChangedEventArgs e)
    {
        if (GrillaProductos.SelectedItem is not Producto producto)
        {
            return;
        }

        _idEnEdicion                   = producto.IdProducto;
        _activoEnEdicion               = producto.Activo;
        CampoId.Text                   = producto.IdProducto.ToString();
        CampoNombre.Text               = producto.NombreProducto;
        ComboCategorias.SelectedValue  = producto.IdCategoria;
        ComboProveedores.SelectedValue = producto.IdProveedor;
        CampoCantidadPorUnidad.Text    = producto.CantidadPorUnidad ?? string.Empty;
        CampoPrecio.Text               = producto.PrecioUnidad.ToString("0.00", CultureInfo.CurrentCulture);
        CampoExistencia.Text           = producto.UnidadesEnExistencia.ToString();
        CampoEnPedido.Text             = producto.UnidadesEnPedido.ToString();
        CampoNivelPedido.Text          = producto.NivelNuevoPedido.ToString();
        CampoSuspendido.IsChecked      = producto.Suspendido;

        TextoModo.Text = producto.Activo
            ? $"PRODUCTO #{producto.IdProducto}"
            : $"PRODUCTO #{producto.IdProducto} (DADO DE BAJA)";

        BotonEliminar.IsEnabled  = producto.Activo;
        BotonRestaurar.IsEnabled = !producto.Activo;
    }

    private void AlNuevo(object sender, RoutedEventArgs e)
    {
        GrillaProductos.SelectedItem = null;
        LimpiarFormulario();
        CampoNombre.Focus();
    }

    private async void AlGuardar(object sender, RoutedEventArgs e)
    {
        if (!Validar(out var producto))
        {
            return;
        }

        try
        {
            if (_idEnEdicion is null)
            {
                var nuevoId = await _productos.InsertarAsync(producto);
                TextoEstado.Text = $"Producto registrado con el ID {nuevoId}.";
            }
            else
            {
                await _productos.ActualizarAsync(producto);
                TextoEstado.Text = $"Producto #{producto.IdProducto} actualizado.";
            }

            await CargarProductosAsync();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo guardar el producto.", ex);
        }
    }

    private bool Validar(out Producto producto)
    {
        producto = new Producto();

        if (string.IsNullOrWhiteSpace(CampoNombre.Text))
        {
            Mensajes.Advertencia("El nombre del producto es obligatorio.");
            CampoNombre.Focus();
            return false;
        }

        if (!decimal.TryParse(CampoPrecio.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var precio)
            || precio < 0)
        {
            Mensajes.Advertencia("El precio unitario debe ser un numero mayor o igual a cero.");
            CampoPrecio.Focus();
            return false;
        }

        if (!LeerCorto(CampoExistencia, "unidades en existencia", out var existencia) ||
            !LeerCorto(CampoEnPedido,    "unidades en pedido",    out var enPedido) ||
            !LeerCorto(CampoNivelPedido, "nivel de nuevo pedido", out var nivel))
        {
            return false;
        }

        producto = new Producto
        {
            IdProducto           = _idEnEdicion ?? 0,
            NombreProducto       = CampoNombre.Text.Trim(),
            IdCategoria          = ComboCategorias.SelectedValue  as int?,
            IdProveedor          = ComboProveedores.SelectedValue as int?,
            CantidadPorUnidad    = CampoCantidadPorUnidad.Text,
            PrecioUnidad         = precio,
            UnidadesEnExistencia = existencia,
            UnidadesEnPedido     = enPedido,
            NivelNuevoPedido     = nivel,
            Suspendido           = CampoSuspendido.IsChecked == true
        };

        return true;
    }

    private static bool LeerCorto(TextBox campo, string descripcion, out short valor)
    {
        if (string.IsNullOrWhiteSpace(campo.Text))
        {
            valor = 0;
            return true;
        }

        if (short.TryParse(campo.Text, out valor) && valor >= 0)
        {
            return true;
        }

        Mensajes.Advertencia($"El campo '{descripcion}' debe ser un numero entero entre 0 y 32767.");
        campo.Focus();
        return false;
    }

    private async void AlEliminar(object sender, RoutedEventArgs e)
    {
        if (_idEnEdicion is null || !_activoEnEdicion)
        {
            return;
        }

        if (!Mensajes.Confirmar(
                $"Se dara de baja el producto #{_idEnEdicion}." + "\n\n" +
                "La fila no se borra: queda con Activo = 0, deja de aparecer en el " +
                "listado y en los combos, y los pedidos historicos la conservan. " +
                "Desea continuar?"))
        {
            return;
        }

        try
        {
            var filas = await _productos.EliminarAsync(_idEnEdicion.Value);
            TextoEstado.Text = $"Producto #{_idEnEdicion} dado de baja ({filas} fila actualizada).";

            await CargarProductosAsync();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo dar de baja el producto.", ex);
        }
    }

    private async void AlRestaurar(object sender, RoutedEventArgs e)
    {
        if (_idEnEdicion is null || _activoEnEdicion)
        {
            return;
        }

        try
        {
            var filas = await _productos.RestaurarAsync(_idEnEdicion.Value);
            TextoEstado.Text = $"Producto #{_idEnEdicion} restaurado ({filas} fila actualizada).";

            await CargarProductosAsync();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo restaurar el producto.", ex);
        }
    }

    private void LimpiarFormulario()
    {
        _idEnEdicion                   = null;
        _activoEnEdicion               = true;
        CampoId.Text                   = "(automatico)";
        CampoNombre.Text               = string.Empty;
        ComboCategorias.SelectedIndex  = -1;
        ComboProveedores.SelectedIndex = -1;
        CampoCantidadPorUnidad.Text    = string.Empty;
        CampoPrecio.Text               = "0.00";
        CampoExistencia.Text           = "0";
        CampoEnPedido.Text             = "0";
        CampoNivelPedido.Text          = "0";
        CampoSuspendido.IsChecked      = false;

        TextoModo.Text           = "NUEVO PRODUCTO";
        BotonEliminar.IsEnabled  = false;
        BotonRestaurar.IsEnabled = false;
    }
}
