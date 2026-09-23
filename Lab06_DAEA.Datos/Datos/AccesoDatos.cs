using System.Data;
using Microsoft.Data.SqlClient;

namespace Lab06_DAEA.Datos;

/// <summary>
/// Acceso a datos con dos modos:
/// - Desconectado (DataSet + SqlDataAdapter.Fill): listados, busquedas y reportes.
/// - Conectado (ExecuteNonQueryAsync): altas, ediciones y bajas logicas.
/// </summary>
public static class AccesoDatos
{
    public static SqlParameter Parametro(string nombre, object? valor)
        => new(nombre, valor ?? DBNull.Value);

    /// <summary>
    /// Modo desconectado: abre la conexion solo el tiempo de Fill, copia el resultado
    /// a un DataSet en memoria y cierra. La UI trabaja sobre esa copia local.
    /// </summary>
    public static async Task<List<T>> ListarAsync<T>(
        string procedimiento,
        Func<DataRow, T> mapear,
        params SqlParameter[] parametros)
    {
        var dataSet = await LlenarDataSetAsync(procedimiento, parametros);

        if (dataSet.Tables.Count == 0)
        {
            return [];
        }

        var resultado = new List<T>(dataSet.Tables[0].Rows.Count);

        foreach (DataRow fila in dataSet.Tables[0].Rows)
        {
            resultado.Add(mapear(fila));
        }

        return resultado;
    }

    public static async Task<T?> ObtenerAsync<T>(
        string procedimiento,
        Func<DataRow, T> mapear,
        params SqlParameter[] parametros) where T : class
        => (await ListarAsync(procedimiento, mapear, parametros)).FirstOrDefault();

    public static async Task<DataSet> LlenarDataSetAsync(
        string procedimiento,
        params SqlParameter[] parametros)
    {
        var dataSet = new DataSet();

        await using var conexion = ConexionBD.Crear();
        await using var comando = new SqlCommand(procedimiento, conexion)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parametros.Length > 0)
        {
            comando.Parameters.AddRange(parametros);
        }

        using var adaptador = new SqlDataAdapter(comando);

        // Fill abre, lee y cierra la conexion: escenario clasico desconectado.
        await Task.Run(() => adaptador.Fill(dataSet));

        return dataSet;
    }

    /// <summary>
    /// Modo conectado: ExecuteNonQuery para mutaciones (insert/update/baja logica).
    /// </summary>
    public static async Task<int> EjecutarNonQueryAsync(
        string procedimiento,
        string nombreSalida,
        params SqlParameter[] parametros)
    {
        await using var conexion = ConexionBD.Crear();
        await using var comando = new SqlCommand(procedimiento, conexion)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parametros.Length > 0)
        {
            comando.Parameters.AddRange(parametros);
        }

        var salida = comando.Parameters.Add(new SqlParameter(nombreSalida, SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        });

        await conexion.OpenAsync();
        await comando.ExecuteNonQueryAsync();

        return salida.Value is null || salida.Value == DBNull.Value
            ? 0
            : Convert.ToInt32(salida.Value);
    }

    public static Task<int> EjecutarInsercionAsync(string procedimiento, params SqlParameter[] parametros)
        => EjecutarNonQueryAsync(procedimiento, "@NuevoId", parametros);

    public static Task<int> EjecutarEscrituraAsync(string procedimiento, params SqlParameter[] parametros)
        => EjecutarNonQueryAsync(procedimiento, "@FilasAfectadas", parametros);

    public static string? LeerTextoNulable(this DataRow fila, string columna)
    {
        if (!fila.Table.Columns.Contains(columna) || fila.IsNull(columna))
        {
            return null;
        }

        return Convert.ToString(fila[columna])?.TrimEnd();
    }

    public static string LeerTexto(this DataRow fila, string columna)
        => fila.LeerTextoNulable(columna) ?? string.Empty;

    public static int? LeerEnteroNulable(this DataRow fila, string columna)
    {
        if (!fila.Table.Columns.Contains(columna) || fila.IsNull(columna))
        {
            return null;
        }

        return Convert.ToInt32(fila[columna]);
    }

    public static int LeerEntero(this DataRow fila, string columna)
        => fila.LeerEnteroNulable(columna) ?? 0;

    public static short LeerCorto(this DataRow fila, string columna)
    {
        if (!fila.Table.Columns.Contains(columna) || fila.IsNull(columna))
        {
            return 0;
        }

        return Convert.ToInt16(fila[columna]);
    }

    public static decimal LeerDecimal(this DataRow fila, string columna)
    {
        if (!fila.Table.Columns.Contains(columna) || fila.IsNull(columna))
        {
            return 0m;
        }

        return Convert.ToDecimal(fila[columna]);
    }

    public static bool LeerBooleano(this DataRow fila, string columna)
    {
        if (!fila.Table.Columns.Contains(columna) || fila.IsNull(columna))
        {
            return false;
        }

        return Convert.ToBoolean(fila[columna]);
    }

    public static DateTime? LeerFechaNulable(this DataRow fila, string columna)
    {
        if (!fila.Table.Columns.Contains(columna) || fila.IsNull(columna))
        {
            return null;
        }

        return Convert.ToDateTime(fila[columna]);
    }
}
