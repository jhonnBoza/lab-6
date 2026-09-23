using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lab06_DAEA.Datos;
using Lab06_DAEA.Datos.Modelos;

namespace Lab06_DAEA.Vistas;

public partial class ProveedoresView : UserControl
{
    private readonly ProveedorRepositorio _repositorio = new();

    private int? _idEnEdicion;
    private bool _activoEnEdicion = true;

    public ProveedoresView() => InitializeComponent();

    private async void AlCargar(object sender, RoutedEventArgs e)
    {
        await BuscarProveedoresAsync();
        LimpiarFormulario();
    }

    private async Task BuscarProveedoresAsync()
    {
        try
        {
            var incluirInactivos = MostrarInactivos.IsChecked == true;

            var proveedores = await _repositorio.BuscarAsync(
                FiltroContacto.Text, FiltroCiudad.Text, incluirInactivos);

            GrillaProveedores.ItemsSource = proveedores;

            var hayFiltros = !string.IsNullOrWhiteSpace(FiltroContacto.Text)
                             || !string.IsNullOrWhiteSpace(FiltroCiudad.Text);

            var dadosDeBaja = proveedores.Count(p => !p.Activo);

            var resumen = hayFiltros
                ? $"{proveedores.Count} proveedor(es) coinciden con el filtro aplicado."
                : $"{proveedores.Count} proveedor(es) en el listado.";

            TextoEstado.Text = dadosDeBaja == 0
                ? resumen
                : $"{resumen} {dadosDeBaja} estan dados de baja.";
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo obtener el listado de proveedores.", ex);
        }
    }

    private async void AlBuscar(object sender, RoutedEventArgs e)
        => await BuscarProveedoresAsync();

    private async void AlCambiarVisibilidad(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        await BuscarProveedoresAsync();
        LimpiarFormulario();
    }

    private async void AlPresionarTeclaFiltro(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await BuscarProveedoresAsync();
        }
    }

    private async void AlLimpiarFiltros(object sender, RoutedEventArgs e)
    {
        FiltroContacto.Text = string.Empty;
        FiltroCiudad.Text   = string.Empty;
        await BuscarProveedoresAsync();
    }

    private void AlSeleccionarProveedor(object sender, SelectionChangedEventArgs e)
    {
        if (GrillaProveedores.SelectedItem is not Proveedor proveedor)
        {
            return;
        }

        _idEnEdicion        = proveedor.IdProveedor;
        _activoEnEdicion    = proveedor.Activo;
        CampoId.Text        = proveedor.IdProveedor.ToString();
        CampoCompania.Text  = proveedor.NombreCompania;
        CampoContacto.Text  = proveedor.NombreContacto ?? string.Empty;
        CampoCargo.Text     = proveedor.CargoContacto  ?? string.Empty;
        CampoDireccion.Text = proveedor.Direccion      ?? string.Empty;
        CampoCiudad.Text    = proveedor.Ciudad         ?? string.Empty;
        CampoRegion.Text    = proveedor.Region         ?? string.Empty;
        CampoCodPostal.Text = proveedor.CodPostal      ?? string.Empty;
        CampoPais.Text      = proveedor.Pais           ?? string.Empty;
        CampoTelefono.Text  = proveedor.Telefono       ?? string.Empty;
        CampoFax.Text       = proveedor.Fax            ?? string.Empty;

        TextoModo.Text = proveedor.Activo
            ? $"PROVEEDOR #{proveedor.IdProveedor}"
            : $"PROVEEDOR #{proveedor.IdProveedor} (DADO DE BAJA)";

        BotonEliminar.IsEnabled  = proveedor.Activo;
        BotonRestaurar.IsEnabled = !proveedor.Activo;
    }

    private void AlNuevo(object sender, RoutedEventArgs e)
    {
        GrillaProveedores.SelectedItem = null;
        LimpiarFormulario();
        CampoCompania.Focus();
    }

    private async void AlGuardar(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CampoCompania.Text))
        {
            Mensajes.Advertencia("El nombre de la compania es obligatorio.");
            CampoCompania.Focus();
            return;
        }

        var proveedor = new Proveedor
        {
            IdProveedor    = _idEnEdicion ?? 0,
            NombreCompania = CampoCompania.Text.Trim(),
            NombreContacto = CampoContacto.Text,
            CargoContacto  = CampoCargo.Text,
            Direccion      = CampoDireccion.Text,
            Ciudad         = CampoCiudad.Text,
            Region         = CampoRegion.Text,
            CodPostal      = CampoCodPostal.Text,
            Pais           = CampoPais.Text,
            Telefono       = CampoTelefono.Text,
            Fax            = CampoFax.Text
        };

        try
        {
            if (_idEnEdicion is null)
            {
                var nuevoId = await _repositorio.InsertarAsync(proveedor);
                TextoEstado.Text = $"Proveedor registrado con el ID {nuevoId}.";
            }
            else
            {
                await _repositorio.ActualizarAsync(proveedor);
                TextoEstado.Text = $"Proveedor #{proveedor.IdProveedor} actualizado.";
            }

            await BuscarProveedoresAsync();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo guardar el proveedor.", ex);
        }
    }

    private async void AlEliminar(object sender, RoutedEventArgs e)
    {
        if (_idEnEdicion is null || !_activoEnEdicion)
        {
            return;
        }

        if (!Mensajes.Confirmar(
                $"Se dara de baja el proveedor #{_idEnEdicion}.\n\n" +
                "La fila no se borra: queda con Activo = 0 y deja de aparecer en la " +
                "busqueda. Podra restaurarlo despues. Desea continuar?"))
        {
            return;
        }

        try
        {
            var filas = await _repositorio.EliminarAsync(_idEnEdicion.Value);
            TextoEstado.Text = $"Proveedor #{_idEnEdicion} dado de baja ({filas} fila actualizada).";

            await BuscarProveedoresAsync();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo dar de baja el proveedor.", ex);
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
            var filas = await _repositorio.RestaurarAsync(_idEnEdicion.Value);
            TextoEstado.Text = $"Proveedor #{_idEnEdicion} restaurado ({filas} fila actualizada).";

            await BuscarProveedoresAsync();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo restaurar el proveedor.", ex);
        }
    }

    private void LimpiarFormulario()
    {
        _idEnEdicion     = null;
        _activoEnEdicion = true;
        CampoId.Text     = "(automatico)";

        foreach (var campo in new[]
                 {
                     CampoCompania, CampoContacto, CampoCargo, CampoDireccion, CampoCiudad,
                     CampoRegion, CampoCodPostal, CampoPais, CampoTelefono, CampoFax
                 })
        {
            campo.Text = string.Empty;
        }

        TextoModo.Text           = "NUEVO PROVEEDOR";
        BotonEliminar.IsEnabled  = false;
        BotonRestaurar.IsEnabled = false;
    }
}
