using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Lab06_DAEA.Datos;
using Lab06_DAEA.Datos.Modelos;

namespace Lab06_DAEA.Vistas;

public partial class ReportesView : UserControl
{
    private readonly ReporteRepositorio _repositorio = new();

    private List<ReporteDetallePedido> _detalle = [];

    public ReportesView() => InitializeComponent();

    private async void AlCargar(object sender, RoutedEventArgs e)
    {
        CampoFechaInicio.SelectedDate = new DateTime(DateTime.Today.Year, 1, 1);
        CampoFechaFin.SelectedDate    = DateTime.Today;

        await GenerarReporteAsync();
    }

    private async void AlGenerar(object sender, RoutedEventArgs e)
        => await GenerarReporteAsync();

    private async void AlUltimos30Dias(object sender, RoutedEventArgs e)
    {
        CampoFechaInicio.SelectedDate = DateTime.Today.AddDays(-30);
        CampoFechaFin.SelectedDate    = DateTime.Today;
        await GenerarReporteAsync();
    }

    private async void AlAnioActual(object sender, RoutedEventArgs e)
    {
        CampoFechaInicio.SelectedDate = new DateTime(DateTime.Today.Year, 1, 1);
        CampoFechaFin.SelectedDate    = new DateTime(DateTime.Today.Year, 12, 31);
        await GenerarReporteAsync();
    }

    private async Task GenerarReporteAsync()
    {
        if (CampoFechaInicio.SelectedDate is not { } fechaInicio ||
            CampoFechaFin.SelectedDate    is not { } fechaFin)
        {
            Mensajes.Advertencia("Seleccione la fecha inicial y la fecha final del reporte.");
            return;
        }

        if (fechaInicio > fechaFin)
        {
            Mensajes.Advertencia("La fecha inicial no puede ser mayor que la fecha final.");
            return;
        }

        try
        {
            _detalle = await _repositorio.DetallesPorFechasAsync(fechaInicio, fechaFin);
            var resumen   = await _repositorio.ResumenPorCategoriaAsync(fechaInicio, fechaFin);
            var excluidos = await _repositorio.PedidosDadosDeBajaAsync(fechaInicio, fechaFin);

            GrillaReporte.ItemsSource = _detalle;
            GrillaResumen.ItemsSource = resumen;

            MostrarTotales(fechaInicio, fechaFin, resumen);
            MostrarExcluidos(excluidos);
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo generar el reporte.", ex);
        }
    }

    private void MostrarTotales(DateTime fechaInicio, DateTime fechaFin, List<ResumenCategoria> resumen)
    {
        var pedidos  = _detalle.Select(d => d.IdPedido).Distinct().Count();
        var importe  = resumen.Sum(r => r.Importe);
        var unidades = resumen.Sum(r => r.UnidadesVendidas);

        TextoTituloDetalle.Text =
            $"Detalle de pedidos del {fechaInicio:dd/MM/yyyy} al {fechaFin:dd/MM/yyyy}";

        TextoEstado.Text = _detalle.Count == 0
            ? "El periodo seleccionado no tiene pedidos registrados."
            : $"{_detalle.Count} linea(s) de detalle en {pedidos} pedido(s).";

        TextoImporteTotal.Text = importe.ToString("N2", CultureInfo.CurrentCulture);
        TextoUnidades.Text     = $"{unidades:N0} unidades vendidas";
    }

    private void MostrarExcluidos(List<PedidoDadoDeBaja> excluidos)
    {
        if (excluidos.Count == 0)
        {
            AvisoExcluidos.Visibility = Visibility.Collapsed;
            return;
        }

        var lineas  = excluidos.Sum(p => p.Lineas);
        var importe = excluidos.Sum(p => p.Importe);
        var ids     = string.Join(", ", excluidos.Select(p => $"#{p.IdPedido}"));

        TextoExcluidos.Text =
            $"El filtro Activo = 1 excluyo {excluidos.Count} pedido(s) dados de baja " +
            $"en este periodo ({ids}): {lineas} linea(s) por " +
            $"{importe.ToString("N2", CultureInfo.CurrentCulture)}. " +
            "Sus filas siguen en la base de datos; solo no se contabilizan.";

        AvisoExcluidos.Visibility = Visibility.Visible;
    }

    private void AlExportar(object sender, RoutedEventArgs e)
    {
        if (_detalle.Count == 0)
        {
            Mensajes.Advertencia("No hay datos que exportar. Genere primero el reporte.");
            return;
        }

        var dialogo = new SaveFileDialog
        {
            Title           = "Exportar reporte",
            Filter          = "Archivo CSV (*.csv)|*.csv",
            FileName        = $"ReporteDetallePedidos_{DateTime.Now:yyyyMMdd_HHmm}.csv",
            OverwritePrompt = true
        };

        if (dialogo.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var contenido = new StringBuilder();
            contenido.AppendLine("IdPedido;FechaPedido;Cliente;Producto;Categoria;PrecioUnidad;Cantidad;Descuento;Subtotal");

            foreach (var fila in _detalle)
            {
                contenido.AppendLine(string.Join(';',
                    fila.IdPedido,
                    fila.FechaPedido?.ToString("dd/MM/yyyy") ?? string.Empty,
                    Escapar(fila.NombreCliente),
                    Escapar(fila.NombreProducto),
                    Escapar(fila.NombreCategoria),
                    fila.PrecioUnidad.ToString("0.00", CultureInfo.InvariantCulture),
                    fila.Cantidad,
                    fila.Descuento.ToString("0.00", CultureInfo.InvariantCulture),
                    fila.Subtotal.ToString("0.00", CultureInfo.InvariantCulture)));
            }

            File.WriteAllText(dialogo.FileName, contenido.ToString(), Encoding.UTF8);
            TextoEstado.Text = $"Reporte exportado a {dialogo.FileName}";
        }
        catch (Exception ex)
        {
            Mensajes.Error("No se pudo exportar el reporte.", ex);
        }
    }

    private static string Escapar(string? texto)
        => (texto ?? string.Empty).Replace(';', ',');
}
