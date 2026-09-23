using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Lab06_DAEA.Datos;
using Lab06_DAEA.Datos.Modelos;

namespace Lab06_DAEA.Vistas;

public partial class PedidosView : UserControl
{
    private readonly PedidoRepositorio   _pedidos   = new();
    private readonly CatalogoRepositorio _catalogos = new();
    private readonly ProductoRepositorio _productos = new();

    private int? _idEnEdicion;
    private bool _activoEnEdicion = true;
    private int? _idProductoEnLinea;

    public PedidosView() => InitializeComponent();

    private async void AlCargar(object sender, RoutedEventArgs e)
    {
        await CargarCombosAsync();
        await CargarPedidosAsync();
        LimpiarFormulario();
    }

    private async Task CargarCombosAsync()
    {
        try
        {
            ComboClientes.ItemsSource  = await _catalogos.ListarClientesAsync();
            ComboEmpleados.ItemsSource = await _catalogos.ListarEmpleadosAsync();
            ComboProductos.ItemsSource = await _productos.ListarAsync();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudieron cargar los catalogos de clientes, empleados y productos.", ex);
        }
    }

    private async Task CargarPedidosAsync()
    {
        try
        {
            var incluirInactivos = MostrarInactivos.IsChecked == true;
            var pedidos = await _pedidos.ListarAsync(incluirInactivos);
            GrillaPedidos.ItemsSource = pedidos;

            var dadosDeBaja = pedidos.Count(p => !p.Activo);

            TextoEstado.Text = dadosDeBaja == 0
                ? $"{pedidos.Count} pedido(s) activos."
                : $"{pedidos.Count} pedido(s), de los cuales {dadosDeBaja} estan dados de baja.";
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo cargar el listado de pedidos.", ex);
        }
    }

    private async void AlCambiarVisibilidad(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        await CargarPedidosAsync();
        LimpiarFormulario();
    }

    private async void AlSeleccionarPedido(object sender, SelectionChangedEventArgs e)
    {
        if (GrillaPedidos.SelectedItem is not Pedido pedido)
        {
            return;
        }

        _idEnEdicion                   = pedido.IdPedido;
        _activoEnEdicion               = pedido.Activo;
        CampoId.Text                   = pedido.IdPedido.ToString();
        ComboClientes.SelectedValue    = pedido.IdCliente?.TrimEnd();
        ComboEmpleados.SelectedValue   = pedido.IdEmpleado;
        CampoFechaPedido.SelectedDate  = pedido.FechaPedido;
        CampoFechaEntrega.SelectedDate = pedido.FechaEntrega;
        CampoFechaEnvio.SelectedDate   = pedido.FechaEnvio;
        CampoCargo.Text                = pedido.Cargo.ToString("0.00", CultureInfo.CurrentCulture);
        CampoDestinatario.Text         = pedido.Destinatario          ?? string.Empty;
        CampoDireccion.Text            = pedido.DireccionDestinatario ?? string.Empty;
        CampoCiudad.Text               = pedido.CiudadDestinatario    ?? string.Empty;
        CampoPais.Text                 = pedido.PaisDestinatario      ?? string.Empty;

        TextoModo.Text = pedido.Activo
            ? $"PEDIDO #{pedido.IdPedido}"
            : $"PEDIDO #{pedido.IdPedido} (DADO DE BAJA)";

        BotonEliminar.IsEnabled  = pedido.Activo;
        BotonRestaurar.IsEnabled = !pedido.Activo;

        await CargarDetalleAsync(pedido.IdPedido);
    }

    private void AlSeleccionarCliente(object sender, SelectionChangedEventArgs e)
    {
        if (_idEnEdicion is not null || ComboClientes.SelectedItem is not Cliente cliente)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(CampoDestinatario.Text))
        {
            CampoDestinatario.Text = cliente.NombreCompania;
        }

        if (string.IsNullOrWhiteSpace(CampoCiudad.Text))
        {
            CampoCiudad.Text = cliente.Ciudad ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(CampoPais.Text))
        {
            CampoPais.Text = cliente.Pais ?? string.Empty;
        }
    }

    private void AlNuevo(object sender, RoutedEventArgs e)
    {
        GrillaPedidos.SelectedItem = null;
        LimpiarFormulario();
        ComboClientes.Focus();
    }

    private async void AlGuardar(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(CampoCargo.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var cargo)
            || cargo < 0)
        {
            Mensajes.Advertencia("El cargo por envio debe ser un numero mayor o igual a cero.");
            CampoCargo.Focus();
            return;
        }

        if (CampoFechaPedido.SelectedDate is null)
        {
            Mensajes.Advertencia("La fecha del pedido es obligatoria.");
            CampoFechaPedido.Focus();
            return;
        }

        var pedido = new Pedido
        {
            IdPedido              = _idEnEdicion ?? 0,
            IdCliente             = ComboClientes.SelectedValue  as string,
            IdEmpleado            = ComboEmpleados.SelectedValue as int?,
            FechaPedido           = CampoFechaPedido.SelectedDate,
            FechaEntrega          = CampoFechaEntrega.SelectedDate,
            FechaEnvio            = CampoFechaEnvio.SelectedDate,
            Cargo                 = cargo,
            Destinatario          = CampoDestinatario.Text,
            DireccionDestinatario = CampoDireccion.Text,
            CiudadDestinatario    = CampoCiudad.Text,
            PaisDestinatario      = CampoPais.Text
        };

        try
        {
            if (_idEnEdicion is null)
            {
                var nuevoId = await _pedidos.InsertarAsync(pedido);
                TextoEstado.Text = $"Pedido registrado con el ID {nuevoId}. Ya puede agregar el detalle.";

                await CargarPedidosAsync();
                SeleccionarPedidoEnGrilla(nuevoId);
            }
            else
            {
                await _pedidos.ActualizarAsync(pedido);
                TextoEstado.Text = $"Pedido #{pedido.IdPedido} actualizado.";

                var idActual = _idEnEdicion.Value;
                await CargarPedidosAsync();
                SeleccionarPedidoEnGrilla(idActual);
            }
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo guardar el pedido.", ex);
        }
    }

    private void SeleccionarPedidoEnGrilla(int idPedido)
    {
        if (GrillaPedidos.ItemsSource is not IEnumerable<Pedido> pedidos)
        {
            return;
        }

        var pedido = pedidos.FirstOrDefault(p => p.IdPedido == idPedido);

        if (pedido is not null)
        {
            GrillaPedidos.SelectedItem = pedido;
            GrillaPedidos.ScrollIntoView(pedido);
        }
    }

    private async void AlEliminar(object sender, RoutedEventArgs e)
    {
        if (_idEnEdicion is null || !_activoEnEdicion)
        {
            return;
        }

        if (!Mensajes.Confirmar(
                $"Se dara de baja el pedido #{_idEnEdicion}.\n\n" +
                "La cabecera queda con Activo = 0 y el pedido desaparece del listado " +
                "y del reporte, pero sus lineas de detalle se conservan. Desea continuar?"))
        {
            return;
        }

        try
        {
            var filas = await _pedidos.EliminarAsync(_idEnEdicion.Value);
            TextoEstado.Text = $"Pedido #{_idEnEdicion} dado de baja ({filas} fila actualizada).";

            await CargarPedidosAsync();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo dar de baja el pedido.", ex);
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
            var filas = await _pedidos.RestaurarAsync(_idEnEdicion.Value);
            TextoEstado.Text = $"Pedido #{_idEnEdicion} restaurado ({filas} fila actualizada).";

            await CargarPedidosAsync();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo restaurar el pedido.", ex);
        }
    }

    private async Task CargarDetalleAsync(int idPedido)
    {
        try
        {
            var lineas = await _pedidos.ListarDetalleAsync(idPedido);
            GrillaDetalle.ItemsSource = lineas;

            TextoTituloDetalle.Text = _activoEnEdicion
                ? $"Detalle del pedido #{idPedido}  ({lineas.Count} linea(s))"
                : $"Detalle del pedido #{idPedido}  ({lineas.Count} linea(s)) - solo lectura, el pedido esta dado de baja";

            TextoTotal.Text = lineas.Sum(l => l.Subtotal).ToString("N2", CultureInfo.CurrentCulture);

            LimpiarLinea();
            BotonAgregarLinea.IsEnabled = _activoEnEdicion;
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo cargar el detalle del pedido.", ex);
        }
    }

    private void AlSeleccionarProductoDetalle(object sender, SelectionChangedEventArgs e)
    {
        if (_idProductoEnLinea is null && ComboProductos.SelectedItem is Producto producto)
        {
            CampoDetallePrecio.Text = producto.PrecioUnidad.ToString("0.00", CultureInfo.CurrentCulture);
        }
    }

    private void AlSeleccionarLinea(object sender, SelectionChangedEventArgs e)
    {
        if (GrillaDetalle.SelectedItem is not DetallePedido linea || !_activoEnEdicion)
        {
            return;
        }

        _idProductoEnLinea         = linea.IdProducto;
        ComboProductos.SelectedValue = linea.IdProducto;
        CampoDetallePrecio.Text    = linea.PrecioUnidad.ToString("0.00", CultureInfo.CurrentCulture);
        CampoDetalleCantidad.Text  = linea.Cantidad.ToString();
        CampoDetalleDescuento.Text = linea.Descuento.ToString("0.00", CultureInfo.CurrentCulture);
        BotonQuitarLinea.IsEnabled = true;
    }

    private async void AlGuardarLinea(object sender, RoutedEventArgs e)
    {
        if (_idEnEdicion is null)
        {
            Mensajes.Advertencia("Primero guarde la cabecera del pedido y luego agregue el detalle.");
            return;
        }

        if (ComboProductos.SelectedValue is not int idProducto)
        {
            Mensajes.Advertencia("Seleccione el producto de la linea.");
            ComboProductos.Focus();
            return;
        }

        if (!decimal.TryParse(CampoDetallePrecio.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var precio)
            || precio < 0)
        {
            Mensajes.Advertencia("El precio de la linea debe ser un numero mayor o igual a cero.");
            CampoDetallePrecio.Focus();
            return;
        }

        if (!short.TryParse(CampoDetalleCantidad.Text, out var cantidad) || cantidad <= 0)
        {
            Mensajes.Advertencia("La cantidad debe ser un numero entero mayor que cero.");
            CampoDetalleCantidad.Focus();
            return;
        }

        if (!decimal.TryParse(CampoDetalleDescuento.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var descuento)
            || descuento < 0 || descuento > 1)
        {
            Mensajes.Advertencia("El descuento debe estar entre 0 y 1 (por ejemplo 0.10 para un 10%).");
            CampoDetalleDescuento.Focus();
            return;
        }

        var linea = new DetallePedido
        {
            IdPedido     = _idEnEdicion.Value,
            IdProducto   = idProducto,
            PrecioUnidad = precio,
            Cantidad     = cantidad,
            Descuento    = descuento
        };

        try
        {
            if (_idProductoEnLinea == idProducto)
            {
                await _pedidos.ActualizarDetalleAsync(linea);
                TextoEstado.Text = "Linea de detalle actualizada.";
            }
            else
            {
                await _pedidos.InsertarDetalleAsync(linea);
                TextoEstado.Text = "Linea de detalle agregada.";
            }

            await CargarDetalleAsync(_idEnEdicion.Value);
            await RefrescarTotalEnGrillaAsync();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo guardar la linea de detalle.", ex);
        }
    }

    private async void AlQuitarLinea(object sender, RoutedEventArgs e)
    {
        if (_idEnEdicion is null || GrillaDetalle.SelectedItem is not DetallePedido linea)
        {
            return;
        }

        if (!Mensajes.Confirmar($"Se quitara '{linea.NombreProducto}' del pedido. Desea continuar?"))
        {
            return;
        }

        try
        {
            await _pedidos.EliminarDetalleAsync(linea.IdPedido, linea.IdProducto);
            TextoEstado.Text = "Linea de detalle eliminada.";

            await CargarDetalleAsync(_idEnEdicion.Value);
            await RefrescarTotalEnGrillaAsync();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo eliminar la linea de detalle.", ex);
        }
    }

    private async Task RefrescarTotalEnGrillaAsync()
    {
        if (_idEnEdicion is null)
        {
            return;
        }

        var idActual = _idEnEdicion.Value;
        await CargarPedidosAsync();
        SeleccionarPedidoEnGrilla(idActual);
    }

    private void LimpiarLinea()
    {
        _idProductoEnLinea           = null;
        GrillaDetalle.SelectedItem   = null;
        ComboProductos.SelectedIndex = -1;
        CampoDetallePrecio.Text      = "0.00";
        CampoDetalleCantidad.Text    = "1";
        CampoDetalleDescuento.Text   = "0.00";
        BotonQuitarLinea.IsEnabled   = false;
    }

    private void LimpiarFormulario()
    {
        _idEnEdicion                   = null;
        _activoEnEdicion               = true;
        CampoId.Text                   = "(automatico)";
        ComboClientes.SelectedIndex    = -1;
        ComboEmpleados.SelectedIndex   = -1;
        CampoFechaPedido.SelectedDate  = DateTime.Today;
        CampoFechaEntrega.SelectedDate = null;
        CampoFechaEnvio.SelectedDate   = null;
        CampoCargo.Text                = "0.00";
        CampoDestinatario.Text         = string.Empty;
        CampoDireccion.Text            = string.Empty;
        CampoCiudad.Text               = string.Empty;
        CampoPais.Text                 = string.Empty;

        TextoModo.Text              = "NUEVO PEDIDO";
        BotonEliminar.IsEnabled     = false;
        BotonRestaurar.IsEnabled    = false;
        BotonAgregarLinea.IsEnabled = true;

        GrillaDetalle.ItemsSource = null;
        TextoTituloDetalle.Text   = "Detalle del pedido  (guarde la cabecera para habilitar el detalle)";
        TextoTotal.Text           = "0.00";

        LimpiarLinea();
    }
}
