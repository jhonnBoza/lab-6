using Microsoft.Data.SqlClient;
using Lab06_DAEA.Datos.Modelos;
using System.Data;

namespace Lab06_DAEA.Datos;

public class ProductoRepositorio
{
    public Task<List<Producto>> ListarAsync(bool incluirInactivos = false)
        => AccesoDatos.ListarAsync("usp_Productos_Listar", Mapear,
               AccesoDatos.Parametro("@IncluirInactivos", incluirInactivos));

    public Task<Producto?> ObtenerAsync(int idProducto)
        => AccesoDatos.ObtenerAsync("usp_Productos_Obtener", Mapear,
               AccesoDatos.Parametro("@IdProducto", idProducto));

    public Task<int> InsertarAsync(Producto producto)
        => AccesoDatos.EjecutarInsercionAsync("usp_Productos_Insertar", Parametros(producto));

    public Task<int> ActualizarAsync(Producto producto)
    {
        var parametros = new List<SqlParameter>
        {
            AccesoDatos.Parametro("@IdProducto", producto.IdProducto)
        };
        parametros.AddRange(Parametros(producto));

        return AccesoDatos.EjecutarEscrituraAsync("usp_Productos_Actualizar", parametros.ToArray());
    }

    public Task<int> EliminarAsync(int idProducto)
        => AccesoDatos.EjecutarEscrituraAsync("usp_Productos_Eliminar",
               AccesoDatos.Parametro("@IdProducto", idProducto));

    public Task<int> RestaurarAsync(int idProducto)
        => AccesoDatos.EjecutarEscrituraAsync("usp_Productos_Restaurar",
               AccesoDatos.Parametro("@IdProducto", idProducto));

    private static SqlParameter[] Parametros(Producto producto) =>
    [
        AccesoDatos.Parametro("@NombreProducto",       producto.NombreProducto),
        AccesoDatos.Parametro("@IdProveedor",          producto.IdProveedor),
        AccesoDatos.Parametro("@IdCategoria",          producto.IdCategoria),
        AccesoDatos.Parametro("@CantidadPorUnidad",
            string.IsNullOrWhiteSpace(producto.CantidadPorUnidad) ? null : producto.CantidadPorUnidad.Trim()),
        AccesoDatos.Parametro("@PrecioUnidad",         producto.PrecioUnidad),
        AccesoDatos.Parametro("@UnidadesEnExistencia", producto.UnidadesEnExistencia),
        AccesoDatos.Parametro("@UnidadesEnPedido",     producto.UnidadesEnPedido),
        AccesoDatos.Parametro("@NivelNuevoPedido",     producto.NivelNuevoPedido),
        AccesoDatos.Parametro("@Suspendido",           producto.Suspendido)
    ];

    private static Producto Mapear(DataRow fila) => new()
    {
        IdProducto           = fila.LeerEntero("IdProducto"),
        NombreProducto       = fila.LeerTexto("NombreProducto"),
        IdProveedor          = fila.LeerEnteroNulable("IdProveedor"),
        NombreProveedor      = fila.LeerTextoNulable("NombreProveedor"),
        IdCategoria          = fila.LeerEnteroNulable("IdCategoria"),
        NombreCategoria      = fila.LeerTextoNulable("NombreCategoria"),
        CantidadPorUnidad    = fila.LeerTextoNulable("CantidadPorUnidad"),
        PrecioUnidad         = fila.LeerDecimal("PrecioUnidad"),
        UnidadesEnExistencia = fila.LeerCorto("UnidadesEnExistencia"),
        UnidadesEnPedido     = fila.LeerCorto("UnidadesEnPedido"),
        NivelNuevoPedido     = fila.LeerCorto("NivelNuevoPedido"),
        Suspendido           = fila.LeerBooleano("Suspendido"),
        Activo               = fila.LeerBooleano("Activo")
    };
}
