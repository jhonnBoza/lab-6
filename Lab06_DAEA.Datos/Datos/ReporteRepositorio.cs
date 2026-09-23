using System.Data;
using Lab06_DAEA.Datos.Modelos;

namespace Lab06_DAEA.Datos;

public class ReporteRepositorio
{
    public Task<List<ReporteDetallePedido>> DetallesPorFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        => AccesoDatos.ListarAsync("usp_Reporte_DetallesPedidosPorFechas", MapearDetalle,
               AccesoDatos.Parametro("@FechaInicio", fechaInicio.Date),
               AccesoDatos.Parametro("@FechaFin",    fechaFin.Date));

    public Task<List<ResumenCategoria>> ResumenPorCategoriaAsync(DateTime fechaInicio, DateTime fechaFin)
        => AccesoDatos.ListarAsync("usp_Reporte_ResumenPorCategoria", MapearResumen,
               AccesoDatos.Parametro("@FechaInicio", fechaInicio.Date),
               AccesoDatos.Parametro("@FechaFin",    fechaFin.Date));

    public Task<List<PedidoDadoDeBaja>> PedidosDadosDeBajaAsync(DateTime fechaInicio, DateTime fechaFin)
        => AccesoDatos.ListarAsync("usp_Reporte_PedidosDadosDeBaja", fila => new PedidoDadoDeBaja
        {
            IdPedido      = fila.LeerEntero("IdPedido"),
            FechaPedido   = fila.LeerFechaNulable("FechaPedido"),
            IdCliente     = fila.LeerTextoNulable("IdCliente"),
            NombreCliente = fila.LeerTextoNulable("NombreCliente"),
            Lineas        = fila.LeerEntero("Lineas"),
            Importe       = fila.LeerDecimal("Importe")
        },
               AccesoDatos.Parametro("@FechaInicio", fechaInicio.Date),
               AccesoDatos.Parametro("@FechaFin",    fechaFin.Date));

    private static ReporteDetallePedido MapearDetalle(DataRow fila) => new()
    {
        IdPedido        = fila.LeerEntero("IdPedido"),
        FechaPedido     = fila.LeerFechaNulable("FechaPedido"),
        IdCliente       = fila.LeerTextoNulable("IdCliente"),
        NombreCliente   = fila.LeerTextoNulable("NombreCliente"),
        IdProducto      = fila.LeerEntero("IdProducto"),
        NombreProducto  = fila.LeerTextoNulable("NombreProducto"),
        NombreCategoria = fila.LeerTextoNulable("NombreCategoria"),
        PrecioUnidad    = fila.LeerDecimal("PrecioUnidad"),
        Cantidad        = fila.LeerCorto("Cantidad"),
        Descuento       = fila.LeerDecimal("Descuento"),
        Subtotal        = fila.LeerDecimal("Subtotal")
    };

    private static ResumenCategoria MapearResumen(DataRow fila) => new()
    {
        NombreCategoria  = fila.LeerTexto("NombreCategoria"),
        Pedidos          = fila.LeerEntero("Pedidos"),
        UnidadesVendidas = fila.LeerEntero("UnidadesVendidas"),
        Importe          = fila.LeerDecimal("Importe")
    };
}

public class CatalogoRepositorio
{
    public Task<List<Cliente>> ListarClientesAsync()
        => AccesoDatos.ListarAsync("usp_Clientes_Listar", fila => new Cliente
        {
            IdCliente      = fila.LeerTexto("IdCliente"),
            NombreCompania = fila.LeerTexto("NombreCompania"),
            NombreContacto = fila.LeerTextoNulable("NombreContacto"),
            Ciudad         = fila.LeerTextoNulable("Ciudad"),
            Pais           = fila.LeerTextoNulable("Pais"),
            Telefono       = fila.LeerTextoNulable("Telefono")
        });

    public Task<List<Empleado>> ListarEmpleadosAsync()
        => AccesoDatos.ListarAsync("usp_Empleados_Listar", fila => new Empleado
        {
            IdEmpleado     = fila.LeerEntero("IdEmpleado"),
            Apellidos      = fila.LeerTexto("Apellidos"),
            Nombre         = fila.LeerTexto("Nombre"),
            NombreCompleto = fila.LeerTexto("NombreCompleto"),
            Cargo          = fila.LeerTextoNulable("Cargo")
        });
}
