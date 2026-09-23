using System.Data;
using Microsoft.Data.SqlClient;
using Lab06_DAEA.Datos.Modelos;

namespace Lab06_DAEA.Datos;

public class PedidoRepositorio
{
    public Task<List<Pedido>> ListarAsync(bool incluirInactivos = false)
        => AccesoDatos.ListarAsync("usp_Pedidos_Listar", MapearPedido,
               AccesoDatos.Parametro("@IncluirInactivos", incluirInactivos));

    public Task<Pedido?> ObtenerAsync(int idPedido)
        => AccesoDatos.ObtenerAsync("usp_Pedidos_Obtener", MapearPedido,
               AccesoDatos.Parametro("@IdPedido", idPedido));

    public Task<int> InsertarAsync(Pedido pedido)
        => AccesoDatos.EjecutarInsercionAsync("usp_Pedidos_Insertar", Parametros(pedido));

    public Task<int> ActualizarAsync(Pedido pedido)
    {
        var parametros = new List<SqlParameter>
        {
            AccesoDatos.Parametro("@IdPedido", pedido.IdPedido)
        };
        parametros.AddRange(Parametros(pedido));

        return AccesoDatos.EjecutarEscrituraAsync("usp_Pedidos_Actualizar", parametros.ToArray());
    }

    public Task<int> EliminarAsync(int idPedido)
        => AccesoDatos.EjecutarEscrituraAsync("usp_Pedidos_Eliminar",
               AccesoDatos.Parametro("@IdPedido", idPedido));

    public Task<int> RestaurarAsync(int idPedido)
        => AccesoDatos.EjecutarEscrituraAsync("usp_Pedidos_Restaurar",
               AccesoDatos.Parametro("@IdPedido", idPedido));

    public Task<List<DetallePedido>> ListarDetalleAsync(int idPedido)
        => AccesoDatos.ListarAsync("usp_DetallesPedidos_ListarPorPedido", MapearDetalle,
               AccesoDatos.Parametro("@IdPedido", idPedido));

    public Task<int> InsertarDetalleAsync(DetallePedido detalle)
        => AccesoDatos.EjecutarEscrituraAsync("usp_DetallesPedidos_Insertar",
               AccesoDatos.Parametro("@IdPedido",     detalle.IdPedido),
               AccesoDatos.Parametro("@IdProducto",   detalle.IdProducto),
               AccesoDatos.Parametro("@PrecioUnidad", detalle.PrecioUnidad),
               AccesoDatos.Parametro("@Cantidad",     detalle.Cantidad),
               AccesoDatos.Parametro("@Descuento",    detalle.Descuento));

    public Task<int> ActualizarDetalleAsync(DetallePedido detalle)
        => AccesoDatos.EjecutarEscrituraAsync("usp_DetallesPedidos_Actualizar",
               AccesoDatos.Parametro("@IdPedido",     detalle.IdPedido),
               AccesoDatos.Parametro("@IdProducto",   detalle.IdProducto),
               AccesoDatos.Parametro("@PrecioUnidad", detalle.PrecioUnidad),
               AccesoDatos.Parametro("@Cantidad",     detalle.Cantidad),
               AccesoDatos.Parametro("@Descuento",    detalle.Descuento));

    public Task<int> EliminarDetalleAsync(int idPedido, int idProducto)
        => AccesoDatos.EjecutarEscrituraAsync("usp_DetallesPedidos_Eliminar",
               AccesoDatos.Parametro("@IdPedido",   idPedido),
               AccesoDatos.Parametro("@IdProducto", idProducto));

    private static SqlParameter[] Parametros(Pedido pedido) =>
    [
        AccesoDatos.Parametro("@IdCliente",             Normalizar(pedido.IdCliente)),
        AccesoDatos.Parametro("@IdEmpleado",            pedido.IdEmpleado),
        AccesoDatos.Parametro("@FechaPedido",           pedido.FechaPedido),
        AccesoDatos.Parametro("@FechaEntrega",          pedido.FechaEntrega),
        AccesoDatos.Parametro("@FechaEnvio",            pedido.FechaEnvio),
        AccesoDatos.Parametro("@Cargo",                 pedido.Cargo),
        AccesoDatos.Parametro("@Destinatario",          Normalizar(pedido.Destinatario)),
        AccesoDatos.Parametro("@DireccionDestinatario", Normalizar(pedido.DireccionDestinatario)),
        AccesoDatos.Parametro("@CiudadDestinatario",    Normalizar(pedido.CiudadDestinatario)),
        AccesoDatos.Parametro("@PaisDestinatario",      Normalizar(pedido.PaisDestinatario))
    ];

    private static string? Normalizar(string? texto)
        => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static Pedido MapearPedido(DataRow fila) => new()
    {
        IdPedido              = fila.LeerEntero("IdPedido"),
        IdCliente             = fila.LeerTextoNulable("IdCliente"),
        NombreCliente         = fila.LeerTextoNulable("NombreCliente"),
        IdEmpleado            = fila.LeerEnteroNulable("IdEmpleado"),
        NombreEmpleado        = fila.LeerTextoNulable("NombreEmpleado"),
        FechaPedido           = fila.LeerFechaNulable("FechaPedido"),
        FechaEntrega          = fila.LeerFechaNulable("FechaEntrega"),
        FechaEnvio            = fila.LeerFechaNulable("FechaEnvio"),
        Cargo                 = fila.LeerDecimal("Cargo"),
        Destinatario          = fila.LeerTextoNulable("Destinatario"),
        DireccionDestinatario = fila.LeerTextoNulable("DireccionDestinatario"),
        CiudadDestinatario    = fila.LeerTextoNulable("CiudadDestinatario"),
        PaisDestinatario      = fila.LeerTextoNulable("PaisDestinatario"),
        TotalPedido           = fila.LeerDecimal("TotalPedido"),
        Activo                = fila.LeerBooleano("Activo")
    };

    private static DetallePedido MapearDetalle(DataRow fila) => new()
    {
        IdPedido       = fila.LeerEntero("IdPedido"),
        IdProducto     = fila.LeerEntero("IdProducto"),
        NombreProducto = fila.LeerTextoNulable("NombreProducto"),
        PrecioUnidad   = fila.LeerDecimal("PrecioUnidad"),
        Cantidad       = fila.LeerCorto("Cantidad"),
        Descuento      = fila.LeerDecimal("Descuento"),
        Subtotal       = fila.LeerDecimal("Subtotal")
    };
}
