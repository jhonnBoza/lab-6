using System.Data;
using Microsoft.Data.SqlClient;
using Lab06_DAEA.Datos.Modelos;

namespace Lab06_DAEA.Datos;

public class ProveedorRepositorio
{
    public Task<List<Proveedor>> ListarAsync(bool incluirInactivos = false)
        => AccesoDatos.ListarAsync("usp_Proveedores_Listar", Mapear,
               AccesoDatos.Parametro("@IncluirInactivos", incluirInactivos));

    public Task<List<Proveedor>> BuscarAsync(string? nombreContacto, string? ciudad, bool incluirInactivos = false)
        => AccesoDatos.ListarAsync("usp_Proveedores_Buscar", Mapear,
               AccesoDatos.Parametro("@NombreContacto", Normalizar(nombreContacto)),
               AccesoDatos.Parametro("@Ciudad", Normalizar(ciudad)),
               AccesoDatos.Parametro("@IncluirInactivos", incluirInactivos));

    public Task<Proveedor?> ObtenerAsync(int idProveedor)
        => AccesoDatos.ObtenerAsync("usp_Proveedores_Obtener", Mapear,
               AccesoDatos.Parametro("@IdProveedor", idProveedor));

    public Task<int> InsertarAsync(Proveedor proveedor)
        => AccesoDatos.EjecutarInsercionAsync("usp_Proveedores_Insertar", Parametros(proveedor));

    public Task<int> ActualizarAsync(Proveedor proveedor)
    {
        var parametros = new List<SqlParameter>
        {
            AccesoDatos.Parametro("@IdProveedor", proveedor.IdProveedor)
        };
        parametros.AddRange(Parametros(proveedor));

        return AccesoDatos.EjecutarEscrituraAsync("usp_Proveedores_Actualizar", parametros.ToArray());
    }

    public Task<int> EliminarAsync(int idProveedor)
        => AccesoDatos.EjecutarEscrituraAsync("usp_Proveedores_Eliminar",
               AccesoDatos.Parametro("@IdProveedor", idProveedor));

    public Task<int> RestaurarAsync(int idProveedor)
        => AccesoDatos.EjecutarEscrituraAsync("usp_Proveedores_Restaurar",
               AccesoDatos.Parametro("@IdProveedor", idProveedor));

    private static SqlParameter[] Parametros(Proveedor proveedor) =>
    [
        AccesoDatos.Parametro("@NombreCompania", proveedor.NombreCompania),
        AccesoDatos.Parametro("@NombreContacto", Normalizar(proveedor.NombreContacto)),
        AccesoDatos.Parametro("@CargoContacto",  Normalizar(proveedor.CargoContacto)),
        AccesoDatos.Parametro("@Direccion",      Normalizar(proveedor.Direccion)),
        AccesoDatos.Parametro("@Ciudad",         Normalizar(proveedor.Ciudad)),
        AccesoDatos.Parametro("@Region",         Normalizar(proveedor.Region)),
        AccesoDatos.Parametro("@CodPostal",      Normalizar(proveedor.CodPostal)),
        AccesoDatos.Parametro("@Pais",           Normalizar(proveedor.Pais)),
        AccesoDatos.Parametro("@Telefono",       Normalizar(proveedor.Telefono)),
        AccesoDatos.Parametro("@Fax",            Normalizar(proveedor.Fax))
    ];

    private static string? Normalizar(string? texto)
        => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static Proveedor Mapear(DataRow fila) => new()
    {
        IdProveedor    = fila.LeerEntero("IdProveedor"),
        NombreCompania = fila.LeerTexto("NombreCompania"),
        NombreContacto = fila.LeerTextoNulable("NombreContacto"),
        CargoContacto  = fila.LeerTextoNulable("CargoContacto"),
        Direccion      = fila.LeerTextoNulable("Direccion"),
        Ciudad         = fila.LeerTextoNulable("Ciudad"),
        Region         = fila.LeerTextoNulable("Region"),
        CodPostal      = fila.LeerTextoNulable("CodPostal"),
        Pais           = fila.LeerTextoNulable("Pais"),
        Telefono       = fila.LeerTextoNulable("Telefono"),
        Fax            = fila.LeerTextoNulable("Fax"),
        Activo         = fila.LeerBooleano("Activo")
    };
}
