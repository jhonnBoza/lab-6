using System.Data;
using Lab06_DAEA.Datos.Modelos;

namespace Lab06_DAEA.Datos;

public class CategoriaRepositorio
{
    public Task<List<Categoria>> ListarAsync(bool incluirInactivos = false)
        => AccesoDatos.ListarAsync("usp_Categorias_Listar", Mapear,
               AccesoDatos.Parametro("@IncluirInactivos", incluirInactivos));

    public Task<Categoria?> ObtenerAsync(int idCategoria)
        => AccesoDatos.ObtenerAsync("usp_Categorias_Obtener", Mapear,
               AccesoDatos.Parametro("@IdCategoria", idCategoria));

    public Task<int> InsertarAsync(Categoria categoria)
        => AccesoDatos.EjecutarInsercionAsync("usp_Categorias_Insertar",
               AccesoDatos.Parametro("@NombreCategoria", categoria.NombreCategoria),
               AccesoDatos.Parametro("@Descripcion", categoria.Descripcion));

    public Task<int> ActualizarAsync(Categoria categoria)
        => AccesoDatos.EjecutarEscrituraAsync("usp_Categorias_Actualizar",
               AccesoDatos.Parametro("@IdCategoria", categoria.IdCategoria),
               AccesoDatos.Parametro("@NombreCategoria", categoria.NombreCategoria),
               AccesoDatos.Parametro("@Descripcion", categoria.Descripcion));

    public Task<int> EliminarAsync(int idCategoria)
        => AccesoDatos.EjecutarEscrituraAsync("usp_Categorias_Eliminar",
               AccesoDatos.Parametro("@IdCategoria", idCategoria));

    public Task<int> RestaurarAsync(int idCategoria)
        => AccesoDatos.EjecutarEscrituraAsync("usp_Categorias_Restaurar",
               AccesoDatos.Parametro("@IdCategoria", idCategoria));

    private static Categoria Mapear(DataRow fila) => new()
    {
        IdCategoria     = fila.LeerEntero("IdCategoria"),
        NombreCategoria = fila.LeerTexto("NombreCategoria"),
        Descripcion     = fila.LeerTextoNulable("Descripcion"),
        Activo          = fila.LeerBooleano("Activo")
    };
}
