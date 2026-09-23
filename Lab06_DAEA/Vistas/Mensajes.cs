using System.Windows;
using Microsoft.Data.SqlClient;

namespace Lab06_DAEA.Vistas;

internal static class Mensajes
{
    private const string Titulo = "NeptunoDB — Lab 06";

    public static void Informacion(string texto)
        => MessageBox.Show(texto, Titulo, MessageBoxButton.OK, MessageBoxImage.Information);

    public static void Advertencia(string texto)
        => MessageBox.Show(texto, Titulo, MessageBoxButton.OK, MessageBoxImage.Warning);

    public static bool Confirmar(string texto)
        => MessageBox.Show(texto, Titulo, MessageBoxButton.YesNo, MessageBoxImage.Question)
           == MessageBoxResult.Yes;

    public static void Error(string contexto, Exception excepcion)
    {
        var detalle = excepcion is SqlException sql
            ? sql.Message
            : $"{excepcion.GetType().Name}: {excepcion.Message}";

        MessageBox.Show($"{contexto}\n\n{detalle}", Titulo,
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
