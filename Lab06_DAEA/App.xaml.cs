using System.Windows;
using Lab06_DAEA.Datos;

namespace Lab06_DAEA;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // async/await de punta a punta: sin .Result ni .Wait() que congelen la UI.
            await ConexionBD.ProbarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "No se pudo conectar con la base de datos NeptunoDB.\n\n" +
                $"Detalle: {ex.Message}\n\n" +
                "Verifique que los scripts de la carpeta Scripts se hayan ejecutado y que la " +
                "cadena de conexion de App.config del proyecto WPF (no de la Class Library) " +
                "apunte a su servidor.",
                "Error de conexion",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
            return;
        }

        var ventana = new MainWindow();
        ventana.Show();
    }
}
