using System.Configuration;
using Microsoft.Data.SqlClient;

namespace Lab06_DAEA.Datos;

/// <summary>
/// Lee la cadena de conexion desde el App.config del proyecto de inicio (WPF).
/// Gotcha: ConfigurationManager busca en el ejecutable, no en la Class Library.
/// </summary>
public static class ConexionBD
{
    private const string NombreCadena = "NeptunoDB";

    public static string CadenaConexion { get; } = LeerCadena();

    private static string LeerCadena()
    {
        var configuracion = ConfigurationManager.ConnectionStrings[NombreCadena];

        if (configuracion is null || string.IsNullOrWhiteSpace(configuracion.ConnectionString))
        {
            throw new ConfigurationErrorsException(
                $"No se encontro la cadena de conexion '{NombreCadena}' en App.config del proyecto de inicio (WPF). " +
                "Si la cadena esta solo en la Class Library, ConfigurationManager no la vera en tiempo de ejecucion.");
        }

        return configuracion.ConnectionString;
    }

    public static SqlConnection Crear() => new(CadenaConexion);

    public static (string Servidor, string BaseDatos) Resumen()
    {
        var builder = new SqlConnectionStringBuilder(CadenaConexion);
        return (builder.DataSource, builder.InitialCatalog);
    }

    public static async Task ProbarAsync()
    {
        await using var conexion = Crear();
        await conexion.OpenAsync();
    }
}
