using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Lab06_DAEA.Datos;

namespace Lab06_DAEA;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MostrarDatosConexion();
    }

    private void MostrarDatosConexion()
    {
        var (servidor, baseDatos) = ConexionBD.Resumen();
        TextoConexion.Text = $"{servidor}  ·  {baseDatos}";
    }

    private void AlCambiarModulo(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, Navegacion))
        {
            return;
        }

        if (Navegacion.Template.FindName("PART_SelectedContentHost", Navegacion)
            is not ContentPresenter contenido)
        {
            return;
        }

        var atenuacion = new DoubleAnimation
        {
            From           = 0,
            To             = 1,
            Duration       = TimeSpan.FromMilliseconds(160),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        contenido.BeginAnimation(OpacityProperty, atenuacion);
    }
}
